using EntityFramework.Audit;
using Microsoft.Data.Edm;
using Playground.Models;
using System;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;

namespace Playground
{
    public static class ODataConfig
    {
        public static void Register(HttpConfiguration config)
        {
            AuditConfiguration.Default.DefaultAuditable = true;
            AuditConfiguration.Default.IncludeRelationships = true;

            config.Routes.MapODataServiceRoute(
                routeName: "OData",
                routePrefix: "odata",
                model: Model.Value
            );
        }

        public static Lazy<IEdmModel> Model = new Lazy<IEdmModel>(BuildModel);

        private static IEdmModel BuildModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.ContainerName = nameof(PlaygroundContext);
            builder.EntitySet<Company>(nameof(PlaygroundContext.Companies));
            builder.EntitySet<Site>(nameof(PlaygroundContext.Sites));

            return builder.GetEdmModel();
        }
    }
}