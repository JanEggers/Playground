using Playground.Models;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.OData;

namespace Playground.Controllers
{
    public class CompaniesController : ODataController  
    {
        private PlaygroundContext m_ctx;

        public CompaniesController(PlaygroundContext ctx )
        {
            m_ctx = ctx;
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
