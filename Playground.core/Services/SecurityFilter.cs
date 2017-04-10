using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization.Infrastructure;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    private readonly IOptions<AuthorizationOptions> authorizationOptions;

    public SecurityRequirementsOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
    {
        this.authorizationOptions = authorizationOptions;
    }

    public void Apply(Swashbuckle.AspNetCore.Swagger.Operation operation, OperationFilterContext context)
    {
        var controllerPolicies = context.ApiDescription.ControllerAttributes()
            .OfType<AuthorizeAttribute>();
        var actionPolicies = context.ApiDescription.ActionAttributes()
            .OfType<AuthorizeAttribute>();
        var authrizations = controllerPolicies.Union(actionPolicies).Distinct();

        if (!authrizations.Any()) {
            return;
        }

        operation.Responses.Add("401", new Response { Description = "Unauthorized" });

        var requiredClaimTypes = authrizations
            .Select(x => x.Policy != null ? authorizationOptions.Value.GetPolicy(x.Policy) : authorizationOptions.Value.DefaultPolicy)
            .SelectMany(x => x.Requirements)
            .OfType<ClaimsAuthorizationRequirement>()
            .Select(x => x.ClaimType);

        if (requiredClaimTypes.Any())
        {
            operation.Responses.Add("403", new Response { Description = "Forbidden" });

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
            operation.Security.Add(
                new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", requiredClaimTypes }
                });
        }
    }
}