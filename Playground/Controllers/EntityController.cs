using Playground.Services;
using System.Data.Entity;
using System.Web.Http;
using System.Web.Http.OData;

namespace Playground.Controllers
{
    public class EntityController : ODataController
    {
        private UpdateService m_updater;

        protected EntityController(UpdateService updater) 
        {
            m_updater = updater;
        }

        protected IHttpActionResult GetSingle<T>(IDbSet<T> set, int key)
                    where T : class
        {
            var item = set.Find(key);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        protected IHttpActionResult Post<T>(T item)
                where T : class
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            m_updater.Context.Set<T>().Add(item);
            m_updater.Context.SaveChanges();

            return Created(item);
        }

        protected IHttpActionResult Patch<T>(int key, Delta<T> patch)
            where T : class
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = m_updater.Context.Set<T>().Find(key);
            if (item == null)
            {
                return NotFound();
            }

            patch.Patch(item);

            m_updater.SaveChanges( Request );

            return Updated(item);
        }

        protected IHttpActionResult Delete<T>(IDbSet<T> set, int key)
            where T : class
        {
            var item = set.Find(key);

            if (item != null)
            {
                set.Remove(item);

                m_updater.Context.SaveChanges();
            }

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }
    }
}