using System;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Playground.core.Models;
using System.Collections.Generic;
using Microsoft.AspNet.OData;

namespace Playground.core.Controllers
{
    public abstract class EntityController<TEntity,TKey> : ODataController
        where TEntity : class
    {
        protected PlaygroundContext m_db;
        protected DbSet<TEntity> m_set;

        protected EntityController(PlaygroundContext db, DbSet<TEntity> set ) 
        {
            m_db = db;
            m_set = set;
        }
        
        protected abstract Expression<Func<TEntity, bool>> Find(TKey key);

        protected abstract TKey GetKey(TEntity item);

        protected ActionResult<IQueryable<TEntity>> GetAll()
        {
            return Ok(m_set.AsNoTracking());
        }

        protected ActionResult<TEntity> GetSingleEntity(TKey key)
        {
            var item = m_set.Where(Find(key)).AsNoTracking().FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        protected ActionResult<TRelated> GetSingleRelated<TRelated>(TKey key, Expression<Func<TEntity, TRelated>> selector)
            where TRelated : class
        {
            var item = m_set.Where(Find(key)).AsNoTracking().FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }

            var items = m_set.Where(Find(key)).Select(selector).AsNoTracking().FirstOrDefault();
            return items;
        }

        protected ActionResult<IQueryable<TRelated>> GetManyRelated<TRelated>(TKey key, Expression<Func<TEntity, IEnumerable<TRelated>>> selector)
            where TRelated : class
        {
            var item = m_set.Where(Find(key)).AsNoTracking().FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }

            var items = m_set.Where(Find(key)).SelectMany(selector).AsNoTracking();

            return Ok(items);
        }

        protected IActionResult PostEntity(TEntity item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            m_set.Add(item);
            m_db.SaveChanges();

            return Created(item);
        }
        
        protected IActionResult PatchEntity(TKey key, Delta<TEntity> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = m_set.Where(Find(key)).FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }

            delta.Patch(item);            

            m_db.SaveChanges();
            
            return Updated(item);
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