using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Connections;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Playground.core.Hubs;
using Microsoft.Extensions.Configuration;

namespace Playground.core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(o => {
                    o.ListenAnyIP(5000);


                    //o.ListenAnyIP(1883, l => l.UseHub<MqttHub>());
                    o.ListenAnyIP(1883, l => l.UseConnectionHandler<MyHubConnectionHandler<MqttHub, MqttHubProtocol>>());
                })
                .UseIISIntegration()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, builder) => {
                    builder
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, builder) => {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
