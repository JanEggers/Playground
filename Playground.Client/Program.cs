using Newtonsoft.Json;
using Playground.Client.Http;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Playground.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Run().Wait();
        }

        private static async Task Run() {
            var httpClient = new HttpClient() { 
                BaseAddress = new Uri("http://localhost:5000")
            };

            var api = new PlaygroundClient( httpClient);
            var api2 = new PlaygroundClient(httpClient);


            while (true)
            {
                await Task.Delay(1000);

                try
                {
                    var result = await Login(api, "someone", "pass");

                    Console.WriteLine("login successful");

                    while (true) {
                        //api2.HttpClient.Timeout = TimeSpan.FromSeconds(10);
                        //api2.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {result.AuthentificationToken}");
                        var companies = await api2.GetCompaniesAsync();

                        var start = DateTime.Now;

                        await Task.WhenAll(companies.Value.Select(async p =>
                        {
                            await Task.Yield();
                            p.Sites = (await api2.GetCompaniesByKey1_SitesAsync(p.Id)).Value;
                        }));

                        var diff = DateTime.Now - start;

                        Console.WriteLine($"got em {diff}");
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static async Task<AuthResult> Login(PlaygroundClient api, string userName, string password)
        {
            throw new NotImplementedException();
            //var message = new HttpRequestMessage(HttpMethod.Post, api.BaseUri + "connect/token");
            //message.Content = new StringContent($"grant_type=password&scope=offline_access&username={userName}&password={password}",
            //                Encoding.UTF8,
            //                "application/x-www-form-urlencoded");

            //var response = await api.HttpClient.SendAsync(message);

            //using (var reader = new JsonTextReader(new StreamReader(await response.Content.ReadAsStreamAsync())))
            //{
            //    var serializer = new JsonSerializer();
            //    var result = (dynamic)serializer.Deserialize(reader);

            //    return new AuthResult()
            //    {
            //        RefreshToken = result.refresh_token,
            //        AuthentificationToken = result.access_token,
            //    };
            //}
        }
    }

    public class AuthResult {
        public string RefreshToken { get; set; }

        public string AuthentificationToken { get; set; }
    }
}