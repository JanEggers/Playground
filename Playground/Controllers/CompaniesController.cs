using Playground.Models;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.OData;
using Playground.Services;

namespace Playground.Controllers
{
    public class CompaniesController : EntityController  
    {
        private PlaygroundContext m_ctx;

        public CompaniesController(PlaygroundContext ctx, UpdateService updater)
            : base(updater)
        {
            m_ctx = ctx;
        }
        
        public IEnumerable<Company> Get()
        {
            return m_ctx.Companies;
        }

        public IHttpActionResult Get([FromODataUri] int key)
        {
            return base.GetSingle( m_ctx.Companies, key );
        }
        
        public IHttpActionResult Post([FromBody]Company item)
        {
            return base.Post(item);
        }
        
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Company> patch)
        {
            return base.Patch(key, patch);
        }
                
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            return base.Delete(m_ctx.Companies, key);
        }
    }
}
