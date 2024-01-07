using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args).ConfigurePlaygroundBuilder();

var app = builder.Build();

await app.ConfigurePlaygroundApp();

await app.RunAsync();
public partial class Program { }
