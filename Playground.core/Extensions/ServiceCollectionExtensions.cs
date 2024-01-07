using JSNLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Playground.core.Controllers;
using Playground.core.GraphQL;
using Playground.core.Hubs;
using Serilog;

namespace Playground.core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayground(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMappings();

        services.AddScoped<SeedService>();
        services.AddTransient<PlaygroundModelBuilder>();

        services.AddDbContext<PlaygroundContext>(o => {
            o.UseSqlServer(configuration.GetConnectionString("PlaygroundContext"))
            .EnableThreadSafetyChecks(false);
        });

        services.AddControllers()
            .AddOData((odata, services) =>
            {
                odata
                       .Count()
                       .Expand()
                       .Filter()
                       .OrderBy()
                       .Select()
                       ;

                odata.AddRouteComponents("odata", services.GetRequiredService<PlaygroundModelBuilder>().GetEdmModel());
            })

        .AddApplicationPart(typeof(SitesController).Assembly);
        ;

        // Register the Identity services.
        services.AddIdentity<PlaygroundUser, IdentityRole>(o => {
            o.Password.RequireDigit = false;
            o.Password.RequireLowercase = false;
            o.Password.RequireUppercase = false;
            o.Password.RequireNonAlphanumeric = false;
            o.Password.RequiredLength = 3;
        })
            .AddEntityFrameworkStores<PlaygroundContext>()
            .AddDefaultTokenProviders()
            ;

        // Configure Identity to use the same JWT claims as OpenIddict instead
        // of the legacy WS-Federation claims it uses by default (ClaimTypes),
        // which saves you from doing the mapping in your authorization controller.
        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
        });

        services.AddOpenIddict(options => {
            options
                .AddCore(core => {
                    // Register the Entity Framework stores.
                    core
                        .UseEntityFrameworkCore()
                        .UseDbContext<PlaygroundContext>();
                })
                .AddServer(server =>
                {
                    server
                        .AllowPasswordFlow()
                        .AllowRefreshTokenFlow()
                        .AcceptAnonymousClients()
                        ;

                    server.RegisterScopes(SecurityRequirementsOperationFilter.DefaultScope);
                })
                ;
        });

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = Constant.AuthenticationScheme;
            o.DefaultChallengeScheme = Constant.AuthenticationScheme;
        })
            .AddOAuth(Constant.AuthenticationScheme, oauth => {
                oauth.CallbackPath = "/";
                oauth.ClientId = "Playground";
                oauth.ClientSecret = "SuperSecret";
                oauth.AuthorizationEndpoint = "/connect/token";
                oauth.TokenEndpoint = "/connect/token";
            });

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            options.AddSecurityDefinition(Constant.AuthenticationScheme, SecurityRequirementsOperationFilter.Scheme);
            options.OperationFilter<SecurityRequirementsOperationFilter>();
            options.OperationFilter<OdataOperationIdFilter>();
            options.DocumentFilter<OdataDocumentFilter>();
        });

        services
            .AddGraphQLServer()
            .RegisterDbContext<PlaygroundContext>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddQueryType<Query>();

        services.AddSignalR();
        return services;
    }

    public static WebApplicationBuilder ConfigurePlaygroundBuilder(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration);

        builder.Logging.AddSerilog(loggerConfig.CreateLogger(), true);

        builder.Services.AddPlayground(builder.Configuration);

        return builder;
    }

    public static async Task<WebApplication> ConfigurePlaygroundApp(this WebApplication app)
    {
        var jsnlogConfiguration = new JsnlogConfiguration();
        app.UseJSNLog(new LoggingAdapter(app.Services.GetRequiredService<ILoggerFactory>()), jsnlogConfiguration);

        app.UseDeveloperExceptionPage();

        app.UseAuthentication();

        app.UseStaticFiles();
        app.UseDefaultFiles();

        app.UseRouting();
        //app.UseAuthorization();

        //app.MapGet("/CompaniesRest", async (HttpContext httpContext, PlaygroundContext context) => {
        //    var companies = context.Companies.AsNoTracking()
        //            .Select(c => $"{{\"{nameof(c.Id)}\": {c.Id}, \"{nameof(c.Name)}\": \"{c.Name}\"  }}")
        //            .ToList();

        //    var writer = httpContext.Response.BodyWriter;
        //    var encoding = Encoding.UTF8;

        //    writer.Write(encoding.GetBytes("["));

        //    foreach (var company in companies)
        //    {
        //        encoding.GetBytes(company, writer);
        //    }
        //    writer.Write(encoding.GetBytes("]"));

        //    await writer.FlushAsync();
        //});

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

        return app;
    }
}
