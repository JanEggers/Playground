using Playground.Models;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.OData;
using Playground.Services;
using System;
using System.Net;

namespace Playground.Controllers
{
    public class SitesController : EntityController  
    {
        private PlaygroundContext m_ctx;

        public SitesController(PlaygroundContext ctx, UpdateService updater)
            : base(updater)
        {
            m_ctx = ctx;
        }
        
        public IEnumerable<Site> Get()
        {
            return m_ctx.Sites;
        }

        public IHttpActionResult Get([FromODataUri] int key)
        {
            return base.GetSingle( m_ctx.Sites, key );
        }
        
        public IHttpActionResult Post([FromBody]Site item)
        {
            return base.Post(item);
        }

        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult CreateLink([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            return base.CreateLink<Site, int>(key, navigationProperty, link);
        }


        public IHttpActionResult Patch([FromODataUri] int key, Delta<Site> patch)
        {
            return base.Patch(key, patch);
        }
                
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            return base.Delete(m_ctx.Sites, key);
        }
    }
}
