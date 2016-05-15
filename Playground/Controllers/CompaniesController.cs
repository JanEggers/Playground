using Playground.Models;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.OData;

using Playground.Hubs;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR;

namespace Playground.Controllers
{
    public class CompaniesController : ODataController  
    {
        private PlaygroundContext m_ctx;
        private IHubContext<IUpdater> m_updater;

        public CompaniesController(PlaygroundContext ctx, IHubContext<IUpdater> updater)
        {
            m_ctx = ctx;
            m_updater = updater;
        }
        
        public IEnumerable<Company> Get()
        {
            return m_ctx.Companies;
        }

        public Company Get([FromODataUri] int key)
        {
            return m_ctx.Companies.Find(key);
        }
        
        public IHttpActionResult Post([FromBody]Company item)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            m_ctx.Companies.Add(item);

            m_ctx.SaveChanges();

            return Created(item);
        }
        
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Company> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = m_ctx.Companies.Find(key);
            if (item == null)
            {
                return NotFound(); 
            }

            patch.Patch(item);
            m_ctx.SaveChanges();

            m_updater.Clients.All.Update("lala", new Dictionary<string, object>());

            return Updated(item);
        }
                
        public void Delete([FromODataUri] int key)
        {
            var item = m_ctx.Companies.Find(key);
            m_ctx.Companies.Remove(item);

            m_ctx.SaveChanges();
        }
    }
}
