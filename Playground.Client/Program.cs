using Newtonsoft.Json;
using Playground.Client.Generated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playground.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Run().Wait();
        }

        private static async Task Run() {
            var api = new MyAPI(new Uri("http://localhost:5000/"));


            while (true)
            {
                await Task.Delay(1000);

                try
                {
                    var result = await Login(api, "someone", "pass");

                    Console.WriteLine("login successful");

                    while (true) {
                        using (var api2 = new MyAPI(new Uri("http://localhost:5000/")))
                        {
                            api2.HttpClient.Timeout = TimeSpan.FromSeconds(10);
                            api2.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {result.AuthentificationToken}");
                            var companies = await api2.ApiCompaniesGetAsync();

                            var start = DateTime.Now;

                            await Task.WhenAll(companies.Select(async p =>
                            {
                                await Task.Yield();
                                p.Sites = await api2.ApiCompaniesByKeySitesGetAsync(p.Id.Value);
                            }));

                            var diff = DateTime.Now - start;

                            Console.WriteLine($"got em {diff}");

                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static async Task<AuthResult> Login(MyAPI api, string userName, string password)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, api.BaseUri + "connect/token");
            message.Content = new StringContent($"grant_type=password&scope=offline_access&username={userName}&password={password}",
                            Encoding.UTF8,
                            "application/x-www-form-urlencoded");

            var response = await api.HttpClient.SendAsync(message);

            using (var reader = new JsonTextReader(new StreamReader(await response.Content.ReadAsStreamAsync())))
            {
                var serializer = new JsonSerializer();
                var result = (dynamic)serializer.Deserialize(reader);

                return new AuthResult()
                {
                    RefreshToken = result.refresh_token,
                    AuthentificationToken = result.access_token,
                };
            }
        }
    }

    public class AuthResult {
        public string RefreshToken { get; set; }

        public string AuthentificationToken { get; set; }
    }
}