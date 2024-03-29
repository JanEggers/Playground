﻿using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OData.Client;
using Microsoft.OData.Extensions.Client;
using Playground.Client.Http;

namespace Playground.Core.Test;
public class OdataClientTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OdataClientTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TestGetCompanies()
    {
        var client = _factory.CreateClient();
        var swaggerClient = new PlaygroundClient(client);
        var companies = await swaggerClient.GetCompaniesAsync();
        Assert.NotEmpty(companies.Value);

        var sc = new ServiceCollection();
        sc.AddODataClient("Playground")
        .AddHttpClient(client);

        using var clientSp = sc.BuildServiceProvider();
        var factory = clientSp.GetRequiredService<IODataClientFactory>();

        var ctx = factory.CreateClient<Default.Container>(new Uri(client.BaseAddress, "/odata"), "Playground");

        var query = ctx.Companies.Expand(p => p.Sites).Where(c => c.Name == "Hallo");

        var odataCompaniesExecution = await query.ExecuteAsync<Models.Company>();
        var odataCompanies = odataCompaniesExecution.ToList();
    }
}
