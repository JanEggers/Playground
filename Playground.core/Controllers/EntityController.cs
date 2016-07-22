using System;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Playground.core.Models;

namespace Playground.core.Controllers
{
    public abstract class EntityController<TEntity,TKey> : Controller
        where TEntity : class
    {
        protected PlaygroundContext m_db;
        protected DbSet<TEntity> m_set;

        protected EntityController(PlaygroundContext db, DbSet<TEntity> set ) 
        {
            m_db = db;
            m_set = set;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_db?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected abstract Expression<Func<TEntity, bool>> Find(TKey key);

        protected abstract TKey GetKey(TEntity item);

        protected IActionResult GetAll()
        {
            return Ok(m_set.AsNoTracking());
        }

        protected IActionResult GetSingleEntity(TKey key)
        {
            var item = m_set.Where(Find(key)).AsNoTracking().FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        protected IActionResult PostEntity(TEntity item, string routeName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            m_set.Add(item);
            m_db.SaveChanges();

            return CreatedAtRoute(routeName, new { key = GetKey(item) }, item);
        }
        
        protected IActionResult PatchEntity(TKey key, [FromBody]JsonPatchDocument<TEntity> patch)
        {
            var item = m_set.Where(Find(key)).FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }
            
            patch.ApplyTo(item, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            m_db.SaveChanges();
            
            return Ok(item);
        }
        
        protected IActionResult DeleteEntity(TKey key)
        {
            var item = m_set.Where(Find(key)).FirstOrDefault();
            if (item != null)
            {
                m_set.Remove(item);

                m_db.SaveChanges();
            }

            return NoContent();
        }
    }
}