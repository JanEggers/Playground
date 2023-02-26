using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Playground.core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayground(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMappings();

        services.AddScoped<SeedService>();
        services.AddTransient<PlaygroundModelBuilder>();

        services.AddDbContext<PlaygroundContext>(o => {
            o.UseSqlServer(configuration.GetConnectionString("PlaygroundContext"));
        });
        return services;
    }
}
