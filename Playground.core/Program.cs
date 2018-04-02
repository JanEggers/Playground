using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Connections;
using Playground.core.Hubs;

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
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
