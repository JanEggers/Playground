using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;

namespace Playground.Services
{
    public static class Helpers
    {
        public static object GetKeyFromUri(HttpRequestMessage request, Uri uri)
        {
            object key = null;

            IHttpRoute route = request.GetRouteData().Route;

            // Create an equivalent self-hosted route. 
            IHttpRoute newRoute = new HttpRoute(route.RouteTemplate,
                new HttpRouteValueDictionary(route.Defaults),
                new HttpRouteValueDictionary(route.Constraints),
                new HttpRouteValueDictionary(route.DataTokens), route.Handler);

            // Create a fake GET request for the link URI.
            var tmpRequest = new HttpRequestMessage(HttpMethod.Get, uri);

            // Send this request through the routing process.
            var routeData = newRoute.GetRouteData(
                request.GetConfiguration().VirtualPathRoot, tmpRequest);

            // If the GET request matches the route, use the path segments to find the key.
            if (routeData != null)
            {
                ODataPath path = tmpRequest.ODataProperties().Path;
                var segment = path.Segments.OfType<KeyValuePathSegment>().FirstOrDefault();
                if (segment != null)
                {
                    // Convert the segment into the key type.
                    key = ODataUriUtils.ConvertFromUriLiteral(
                        segment.Value, ODataVersion.V3);
                }
            }

            return key;
        }
    }
}