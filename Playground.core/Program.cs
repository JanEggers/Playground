using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Playground.core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using var host = BuildWebHost(args);

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((ctx, l) => {
                    l.AddConsole();
                    l.AddConfiguration(ctx.Configuration);
                    l.AddSerilog(ctx.HostingEnvironment.ConfigureSerilog(), true);
                })
                .UseStartup<Startup>()
                .Build();
    }
}
