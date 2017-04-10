using JSNLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


using AspNet.Security.OpenIdConnect.Primitives;

using Playground.core.Models;
using Swashbuckle.AspNetCore.Swagger;
using Playground.core.Services;

namespace Playground.core
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            AutoMapperConfig.Initialize();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            
            services.AddDbContext<PlaygroundContext>(o => {
                o.UseSqlServer(Configuration.GetSection("PlaygroundContext:ConnectionString").Value);
    
                o.UseOpenIddict();
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
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<PlaygroundContext>();

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();

                // Enable the authorization, logout, token and userinfo endpoints.
                options
                        //.EnableAuthorizationEndpoint("/Account/Login")
                       //.EnableLogoutEndpoint("/Account/Logout")
                       .EnableTokenEndpoint("/connect/token")
                       //.EnableUserinfoEndpoint("/api/userinfo")
                       ;

                options.AllowPasswordFlow()
                    .AllowRefreshTokenFlow();

                // Make the "client_id" parameter mandatory when sending a token request.
                //options.RequireClientIdentification();

                options.DisableHttpsRequirement();
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });

                options.AddSecurityDefinition("default", new OAuth2Scheme()
                {
                    Flow = "password",
                    TokenUrl = "/connect/token"
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            services.AddTransient<SeedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            
            var jsnlogConfiguration = new JsnlogConfiguration();
            app.UseJSNLog( new LoggingAdapter( loggerFactory ), jsnlogConfiguration );

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddDebug();
            }

            //dont use identity as it creates cookies
            //app.UseIdentity();

            app.UseOAuthValidation();

            app.UseOpenIddict();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value != "/")
                {
                    await next();
                }
                else
                {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.ApplicationServices.GetRequiredService<SeedService>().Seed();
        }
    }
}
