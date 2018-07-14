using System.IO;
using JSNLog;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Primitives;

using Playground.core.Models;
using Playground.core.Hubs;
using Playground.core.Services;

using Serilog;
using Serilog.Events;

using Swashbuckle.AspNetCore.Swagger;
using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.Serializer;
using Playground.core.Mqtt.Signalr;
using Playground.core.Mqtt;
using MQTTnet.Server;
using MQTTnet.Adapter;
using System.Linq;
using MQTTnet.Diagnostics;

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
                o.UseSqlServer(Configuration.GetSection("PlaygroundContext:ConnectionString").Value);
    
                //o.UseOpenIddict();
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
                    // Register the Entity Framework stores.
                    .AddEntityFrameworkCoreStores<PlaygroundContext>()
                    // Register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    .AddMvcBinders()
                    // Enable the authorization, logout, token and userinfo endpoints.
                    .EnableTokenEndpoint("/connect/token")
                    .AllowPasswordFlow()
                    .AllowRefreshTokenFlow()
                    .DisableHttpsRequirement()
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
                options.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });

                options.AddSecurityDefinition("default", new OAuth2Scheme()
                {
                    Flow = "password",
                    TokenUrl = "/connect/token"
                });

                options.DocumentFilter<CustomSchemaFilter>();
                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            services.AddSignalR();

            services.AddTransient<SeedService>();

            services.AddSingleton(typeof(MqttHubConnectionHandler<>), typeof(MqttHubConnectionHandler<>));
            services.AddSingleton(typeof(MqttConnectionHandler), typeof(MqttConnectionHandler));
            services.AddSingleton<MqttHubProtocol>();
            services.AddSingleton<MqttPacketSerializer>();

            services.AddHostedMqttServer(o => {

            })
            .AddConnections()
            .AddMqttConnectionHandler();


            MqttNetGlobalLogger.LogMessagePublished += (sender, e) =>
            {
                if (e.TraceMessage.Level >= MqttNetLogLevel.Warning)
                {

                }
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddSerilog();

            var jsnlogConfiguration = new JsnlogConfiguration();
            app.UseJSNLog( new LoggingAdapter( loggerFactory ), jsnlogConfiguration );

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //loggerFactory.AddDebug();
            }

            //ConfigureSeriLog( env );

            app.UseAuthentication();

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

            app.UseSignalR(routes =>
            {
                routes.MapHub<UpdateHub>("/updates");
            });

            app.UseMqttServer(s =>
            {
                s.ApplicationMessageReceived += (sender, e) =>
                {

                };
            });
        }

        private void ConfigureSeriLog( IHostingEnvironment env )
        {
            string directory;
            if ( env.IsDevelopment() )
            {
                directory = $"{env.ContentRootPath}\\Tracing\\";
            }
            else
            {
                directory = $"{env.ContentRootPath}\\..\\Tracing\\";
            }

            if ( !Directory.Exists( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            var config = new LoggerConfiguration()
                .WriteTo.RollingFile( $"{directory}{{Date}}.log", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}\t[{Level:w3}]\t{Message}\t[{SourceContext}]{NewLine}{Exception}" );

            config.MinimumLevel.Is( LogEventLevel.Debug );

            var logger = config.CreateLogger();
            Log.Logger = logger;
        }
    }
}
