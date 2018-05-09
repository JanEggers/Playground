using Microsoft.Extensions.Logging;
using MQTTnet.Packets;
using MQTTnet.Serializer;
using Newtonsoft.Json;
using Playground.Client.Mqtt.Tcp;
using Playground.core.Mqtt;
using Playground.core.Mqtt.Signalr;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Playground.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            RunMqtt().Wait();
        }

        private static async Task RunMqtt()
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Loopback, 1883);
                var loggerFactory = new LoggerFactory();
                var connection = new TcpConnection(endpoint);
                var serializer = new MqttPacketSerializer();
                var mqtt = new MqttConnectionContext(serializer, connection);

                //var connection = new HubConnectionBuilder()
                //   .ConfigureLogging(logging =>
                //   {
                //       logging.AddConsole();
                //   })
                //   .WithEndPoint(endpoint)
                //   .ConfigureServices(s => {
                //       s.AddTransient<IHubProtocol, MqttHubProtocol>();
                //       s.AddSingleton<MqttPacketSerializer>();
                //   })
                //   .Build();

                while (true)
                {
                    try
                    {
                        await connection.StartAsync();
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }

                try
                {
                    await mqtt.ConnectAsync(new MqttConnectPacket());
                    //await mqtt.SubscribeAsync(new MQTTnet.Packets.MqttSubscribePacket()
                    //{
                    //    TopicFilters = new List<TopicFilter>() { new TopicFilter("#", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) }
                    //});
                }
                catch (Exception)
                {

                    throw;
                }

                var sw = new Stopwatch();

                sw.Start();

                long payload = 0;
                while (true)
                {
                    var p = new MQTTnet.Packets.MqttPublishPacket()
                    {
                        Topic = "Step",
                        Payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    };

                    payload++;

                    await mqtt.PublishAsync(p);

                    if (sw.Elapsed > TimeSpan.FromSeconds(5))
                    {
                        sw.Stop();
                        var elapsed = sw.Elapsed;

                        Console.WriteLine($"send {payload / elapsed.TotalSeconds} packages/sec");

                        sw.Restart();
                        payload = 0;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        //private static async Task Run() {
        //    var api = new MyAPI(new Uri("http://localhost:5000/"));


        //    while (true)
        //    {
        //        await Task.Delay(1000);

        //        try
        //        {
        //            var result = await Login(api, "someone", "pass");

        //            Console.WriteLine("login successful");

        //            while (true) { 
        //                var api2 = new MyAPI(new Uri("http://localhost:5000/"));
        //                api2.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {result.AuthentificationToken}");
        //                var companies = await api2.ApiCompaniesGetAsync();

        //                await Task.WhenAll(companies.Select(async p =>
        //                {
        //                    await Task.Yield(); 
        //                    p.Sites = await api2.ApiCompaniesByKeySitesGetAsync(p.Id.Value);
        //                }));


        //                Console.WriteLine("got em");
        //            }
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //}

        //private static async Task<AuthResult> Login(MyAPI api, string userName, string password)
        //{
        //    var message = new HttpRequestMessage(HttpMethod.Post, api.BaseUri + "connect/token");
        //    message.Content = new StringContent($"grant_type=password&scope=offline_access&username={userName}&password={password}",
        //                    Encoding.UTF8,
        //                    "application/x-www-form-urlencoded");

        //    var response = await api.HttpClient.SendAsync(message);

        //    using (var reader = new JsonTextReader(new StreamReader(await response.Content.ReadAsStreamAsync())))
        //    {
        //        var serializer = new JsonSerializer();
        //        var result = (dynamic)serializer.Deserialize(reader);

        //        return new AuthResult()
        //        {
        //            RefreshToken = result.refresh_token,
        //            AuthentificationToken = result.access_token,
        //        };
        //    }
        //}
    }

    public class AuthResult {
        public string RefreshToken { get; set; }

        public string AuthentificationToken { get; set; }
    }
}