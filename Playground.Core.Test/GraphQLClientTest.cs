using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Query.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OData.Extensions.Client;
using Playground.Client.GraphQL;
using Playground.Client.GraphQL2;
using Playground.Client.Http;
using StrawberryShake.Transport.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Company = Playground.core.Models.Company;

namespace Playground.Core.Test;
public class GraphQLClientTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GraphQLClientTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public class CompaniesResponse
    {
        public Company[] Companies { get; set; }
    }

    [Fact]
    public async Task TestGetCompanies()
    {
        var client = _factory.CreateClient();

        var querycontext = new QueryContext(client);

        var graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions() { EndPoint = new Uri("http://localhost/graphql") }, new NewtonsoftJsonSerializer(), client);
        //var query = new Query<Company>("companies", new()
        //{
        //    Formatter = CamelCasePropertyNameFormatter.Format
        //})
        //    .AddField(p => p.Name)
        //    .AddField(p => p.Sites, sites => 
        //        sites.AddField(p => p.Name))                
        //    .AddArgument("where", """{name: { contains: "Hallo" }}""")
        //    .AddArgument("order", """{name: DESC }""")
        //    .Build();

        //var linq = new List<Company>().AsQueryable().Where(c => c.Name.Contains("Hallo"));

        //companies(where: "{name: { contains: \"Hallo\" }}", order: "{name: DESC }"){ name sites{ name} }

        var compayRequest = new GraphQLRequest
        {
            Query = """
            {
                companies(where: { name: { contains: "Hallo" } }, order: { name: DESC }) {
                    name,
                    sites {
                        name
                    }
                }
            }
            """
        };

        //var query = querycontext.Companies(new CompanyFilterInput()
        //{
        //    Name = new StringOperationFilterInput() { Contains = "Hallo" }
        //}, new List<CompanySortInput>() {
        //  new CompanySortInput(){
        //  Name = SortEnumType.DESC
        //  }
        //}).Include(p => p.Sites);

        //var queryText = query.Query;

        //var response = await query.ToEnumerable();

        //var compayRequest = new GraphQLRequest
        //{
        //    Query = $"{{{query}}}"
        //};

        var response = await graphQLClient.SendQueryAsync<CompaniesResponse>(compayRequest);

        //Assert.NotEmpty(response.Data.Companies);

        var serviceCollection = new ServiceCollection();
        var clientBuilder = serviceCollection.AddPlaygroundClient();
        clientBuilder.ConfigureHttpClient(http => http.BaseAddress = new Uri("http://localhost/graphql"));
        serviceCollection.AddSingleton<IHttpClientFactory>(sp => new HttpClientFactoryMock(client));

        var services = serviceCollection.BuildServiceProvider();

        var tclient = services.GetRequiredService<IPlaygroundClient>();
        var response2 = await tclient.GetCompanies.ExecuteAsync();
    }

    public class HttpClientFactoryMock : IHttpClientFactory 
    {
        private readonly HttpClient _client;

        public HttpClientFactoryMock(HttpClient client) {
            _client = client;
        }

        public HttpClient CreateClient(string name)
        {
            return _client;
        }
    }
}