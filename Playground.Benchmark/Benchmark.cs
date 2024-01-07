using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.Client.Http;
using Playground.core.Extensions;

namespace Playground.Benchmark;

public class Benchmark
{
    WebApplication _server;
    ServiceProvider _client;
    PlaygroundClient _playgroundClient;

    [GlobalSetup]
    public void Setup() 
    {
        var builder = WebApplication.CreateBuilder()
            .ConfigurePlaygroundBuilder();

        builder.WebHost.ConfigureKestrel(k => k.ListenAnyIP(1234));

        _server = builder.Build()
            .ConfigurePlaygroundApp()
            .GetAwaiter().GetResult();
        _server.Start();

        _client = new ServiceCollection()
            .AddHttpClient<PlaygroundClient>(client => client.BaseAddress = new Uri("http://localhost:1234/"))
            .Services.BuildServiceProvider();

        _playgroundClient = _client.GetRequiredService<PlaygroundClient>();
    }

    [Benchmark]
    public async Task<ICollection<Company>> GetSitesAsync() 
        => await _playgroundClient.CompaniesRestAsync(99999);
}
