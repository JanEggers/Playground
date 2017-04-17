using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.OData.Edm;
using Playground.core.Models;

namespace Playground.core.Odata
{
    public class PlaygroundModelBuilder
    {
        private readonly ApplicationPartManager m_applicationPartManager;
        public PlaygroundModelBuilder(ApplicationPartManager applicationPartsManager)
        {
            m_applicationPartManager = applicationPartsManager;
        }

        public IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder(m_applicationPartManager);
            builder.EntitySet<Company>(nameof(PlaygroundContext.Companies));
            builder.EntitySet<Site>(nameof(PlaygroundContext.Sites));
            
            return builder.GetEdmModel();
        }
    }
}
