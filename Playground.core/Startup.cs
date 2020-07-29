using JSNLog;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Primitives;

using Playground.core.Models;
using Playground.core.Hubs;
using Playground.core.Services;
using Microsoft.AspNet.OData.Extensions;
using Playground.core.Odata;
using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNet.OData.Formatter;

namespace Playground.core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMappings();

            // Add framework services.
            services.AddMvc();
            
            services.AddDbContext<PlaygroundContext>(o => {
                o.UseSqlServer(Configuration.GetConnectionString("PlaygroundContext"));
                o.UseLazyLoadingProxies();
            });

            // Register the Identity services.
            services.AddIdentity<PlaygroundUser, IdentityRole>(o => {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 3;
            })
                .AddEntityFrameworkStores<PlaygroundContext>()
                .AddDefaultTokenProviders()
                ;
            
            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            services.AddOpenIddict(options => {
                options
                    .AddCore(core => {
                        // Register the Entity Framework stores.
                        core
                            .UseEntityFrameworkCore()
                            .UseDbContext<PlaygroundContext>();
                    })
                    .AddServer(server =>
                    {
                        // Register the ASP.NET Core MVC binder used by OpenIddict.
                        // Note: if you don't call this method, you won't be able to
                        // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                        server.UseMvc();

                        server
                            // Enable the authorization, logout, token and userinfo endpoints.
                            .EnableTokenEndpoint("/connect/token")
                            .AllowPasswordFlow()
                            .AllowRefreshTokenFlow()
                            .DisableHttpsRequirement()
                            .AcceptAnonymousClients()
                            ;

                        server.RegisterScopes(SecurityRequirementsOperationFilter.DefaultScope);
                    })                    
                    ;
            });

            services.AddAuthentication( o =>
                {
                    o.DefaultAuthenticateScheme = OAuthValidationDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = OAuthValidationDefaults.AuthenticationScheme;
                } )
                .AddOAuthValidation();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                                
                options.AddSecurityDefinition(OAuthValidationDefaults.AuthenticationScheme, SecurityRequirementsOperationFilter.Scheme);
                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            services.AddSignalR();

            services.AddScoped<SeedService>();
            services.AddTransient<PlaygroundModelBuilder>();

            services.AddOData();

            services.AddMvcCore(options => {


                // Workaround: https://github.com/OData/WebApi/issues/1177
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                //endofworkaround
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var jsnlogConfiguration = new JsnlogConfiguration();
            app.UseJSNLog( new LoggingAdapter( loggerFactory ), jsnlogConfiguration );

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseDefaultFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints
                    .Count()
                    .Expand()
                    .Filter()
                    .OrderBy()
                    .Select()
                    ;

                var edmModel = app.ApplicationServices.GetRequiredService<PlaygroundModelBuilder>().GetEdmModel();
                endpoints.MapODataRoute("odata", "odata", edmModel);

                endpoints.MapControllers();

                endpoints.MapHub<UpdateHub>("/updates");
            });
            
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<SeedService>().Seed();
            }
        }
    }
}
