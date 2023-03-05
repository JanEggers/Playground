using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OData.Extensions.Client;
using Playground.Client.Http;
using System;
using System.Collections.Generic;
using System.Linq;
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

        var graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions() { EndPoint = new Uri("http://localhost/graphql") }, new NewtonsoftJsonSerializer(), client);

        var compayRequest = new GraphQLRequest
        {
            Query = """
            {
                companies(where: {name: { contains: "Hallo" }}, order: { name: DESC }) {
                name,
                sites {
                  name
                }
              }
            }
            """
        };

        var response = await graphQLClient.SendQueryAsync<CompaniesResponse>(compayRequest);

        Assert.NotEmpty(response.Data.Companies);
    }
}