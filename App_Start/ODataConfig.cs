using Playground.Models;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;

namespace Playground
{
    public static class ODataConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Company>(nameof(PlaygroundContext.Companies));
            builder.EntitySet<Site>(nameof(PlaygroundContext.Sites));

            config.Routes.MapODataServiceRoute(
                routeName: "OData",
                routePrefix: "odata",
                model: builder.GetEdmModel()
            );
        }
    }
}