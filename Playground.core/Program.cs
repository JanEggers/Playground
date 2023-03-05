using JSNLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Playground.core.Hubs;
using Serilog;
using static OpenIddict.Abstractions.OpenIddictConstants.Permissions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration);

builder.Logging.AddSerilog(loggerConfig.CreateLogger(), true);

builder.Services.AddPlayground(builder.Configuration);

var app = builder.Build();

var jsnlogConfiguration = new JsnlogConfiguration();
app.UseJSNLog(new LoggingAdapter(app.Services.GetRequiredService<ILoggerFactory>()), jsnlogConfiguration);

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();

app.UseStaticFiles();
app.UseDefaultFiles();

app.UseRouting();

app.UseAuthorization();


app.MapGraphQL();

app.MapControllers();
app.MapHub<UpdateHub>("/updates");

app.UseSwagger();
app.UseSwaggerUI();

//app.UseSpa(spa =>
//{
//    if (builder.Environment.IsDevelopment())
//    {
//        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
//    }
//});

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<SeedService>().SeedAsync();
}

await app.RunAsync();
public partial class Program { }
