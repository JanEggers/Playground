using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.AspNetCore.Client;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Serializer;
using Newtonsoft.Json;
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
                var factory = new MqttFactory();
                var endpoint = new IPEndPoint(IPAddress.Loopback, 1883);

                var client = factory.CreateMqttClient(new MqttClientConnectionContextFactory());
                
                while (true)
                {
                    try
                    {
                        var options = new MqttClientOptionsBuilder()
                            .WithTcpServer("localhost", 1883)
                            .Build();

                        await client.ConnectAsync(options);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }

                var sw = new Stopwatch();

                sw.Start();

                long payload = 0;
                while (true)
                {
                    var p = new MqttApplicationMessage()
                    {
                        Topic = "Step",
                        Payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    };

                    payload++;

                    await client.PublishAsync(p);

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