using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.OpenApi.Models;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    private readonly IOptions<AuthorizationOptions> authorizationOptions;

    public SecurityRequirementsOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
    {
        this.authorizationOptions = authorizationOptions;
    }


    public static OpenApiSecurityScheme Scheme { get; } = new OpenApiSecurityScheme()
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            Password = new OpenApiOAuthFlow()
            {
                TokenUrl = new System.Uri("/connect/token", System.UriKind.Relative),
            }
        }
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authrizations = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();

        if (!authrizations.Any()) {
            return;
        }

        operation.Responses.Add( "401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.Add( "403", new OpenApiResponse { Description = "Forbidden" } );

        var requiredClaimTypes = authrizations
            .Select(x => x.Policy != null ? authorizationOptions.Value.GetPolicy(x.Policy) : authorizationOptions.Value.DefaultPolicy)
            .SelectMany(x => x.Requirements)
            .OfType<ClaimsAuthorizationRequirement>()
            .Select(x => x.ClaimType)
            .ToList();

        if (!requiredClaimTypes.Any())
        {
            requiredClaimTypes.Add( "default" );
        }


        var requirement = new OpenApiSecurityRequirement();
        requirement[Scheme] = requiredClaimTypes;

        operation.Security = new List<OpenApiSecurityRequirement>();
        operation.Security.Add(requirement);
    }
}