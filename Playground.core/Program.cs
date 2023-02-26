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

var builder = WebApplication.CreateBuilder();

builder.Logging.ClearProviders();

var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration);

builder.Logging.AddSerilog(loggerConfig.CreateLogger(), true);

builder.Services.AddPlayground(builder.Configuration);

builder.Services.AddControllers()
    .AddOData((odata, services) => {
        odata
               .Count()
               .Expand()
               .Filter()
               .OrderBy()
               .Select()
               ;

        odata.AddRouteComponents(services.GetRequiredService<PlaygroundModelBuilder>().GetEdmModel());
    });

// Register the Identity services.
builder.Services.AddIdentity<PlaygroundUser, IdentityRole>(o => {
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
builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserNameClaimType = Claims.Name;
    options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
    options.ClaimsIdentity.RoleClaimType = Claims.Role;
});

builder.Services.AddOpenIddict(options => {
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

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = Constant.AuthenticationScheme;
    o.DefaultChallengeScheme = Constant.AuthenticationScheme;
})
    .AddOAuth(Constant.AuthenticationScheme, oauth => {
        oauth.CallbackPath= "/";
        oauth.ClientId= "Playground";
        oauth.ClientSecret = "SuperSecret";
        oauth.AuthorizationEndpoint = "/connect/token";
        oauth.TokenEndpoint = "/connect/token";
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    options.AddSecurityDefinition(Constant.AuthenticationScheme, SecurityRequirementsOperationFilter.Scheme);
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddSignalR();

builder.Services.AddMvcCore(options => {


    // Workaround: https://github.com/OData/WebApi/issues/1177
    foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
    {
        outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
    }
    foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
    {
        inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
    }
    //endofworkaround
});


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

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    endpoints.MapHub<UpdateHub>("/updates");
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseSpa(spa =>
{
    if (builder.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
    }
});

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<SeedService>().SeedAsync();
}

await app.RunAsync();
