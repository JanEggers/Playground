using JSNLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Playground.core.Models;

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
            
            services.AddDbContext<PlaygroundContext>(o => 
                o.UseSqlServer(Configuration.GetSection("PlaygroundContext:ConnectionString").Value));

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

            services.AddOpenIddict<PlaygroundUser, PlaygroundContext>()
                .EnableTokenEndpoint("/token")
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow()
                .DisableHttpsRequirement()
                .AddEphemeralSigningKey()
                ;
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

            app.UseIdentity();

            app.UseOAuthValidation();

            app.UseOpenIddict();

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value != "/" )
                {
                    await next();
                }
                else
                {                    
                    context.Response.Redirect("/index.html");
                }
            });
        }
    }
}
