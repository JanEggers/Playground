using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Playground.core.Models;
using System;

namespace Playground.core.Odata
{
    public class PlaygroundModelBuilder : ODataConventionModelBuilder
    {
        public PlaygroundModelBuilder(IServiceProvider serviceProvider)
           : base(serviceProvider)
        {
            EntitySet<Company>(nameof(PlaygroundContext.Companies));
            EntitySet<Site>(nameof(PlaygroundContext.Sites));
        }
    }
}
