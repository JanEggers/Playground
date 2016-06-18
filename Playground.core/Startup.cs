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

            var connection = @"Server=(localdb)\mssqllocaldb;Database=PlaygroundContext;Trusted_Connection=True;";
            services.AddDbContext<PlaygroundContext>(options => options.UseSqlServer(connection));

            // Register the Identity services.
            services.AddIdentity<PlaygroundUser, IdentityRole>()
                .AddEntityFrameworkStores<PlaygroundContext>()
                .AddDefaultTokenProviders();

            // Register the OpenIddict services
            services.AddOpenIddict<PlaygroundUser, PlaygroundContext>()
                .DisableHttpsRequirement();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
            
            app.UseIdentity();

            app.UseOpenIddict();

            app.UseStaticFiles();

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
