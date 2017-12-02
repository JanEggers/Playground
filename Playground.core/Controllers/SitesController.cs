using System.Linq.Expressions;
using System;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

using Playground.core.Models;
using System.Collections.Generic;
using Microsoft.AspNet.OData;

namespace Playground.core.Controllers
{
    //[Authorize]
    [EnableQuery]
    public class SitesController : EntityController<Site, int>  
    {
        public SitesController(PlaygroundContext ctx)
            : base(ctx, ctx.Sites)
        {
        }
        
        protected override Expression<Func<Site, bool>> Find(int key)
        {
            return p => p.Id == key;
        }

        protected override int GetKey(Site item)
        {
            return item.Id;
        }

        [Produces(typeof(IEnumerable<Site>))]
        [HttpGet]
        public IActionResult Get()
        {
            return GetAll();
        }

        [Produces(typeof(Site))]
        public IActionResult Get(int key)
        {
            return GetSingleEntity(key);
        }

        [Produces(typeof(Company))]
        public IActionResult GetCompany(int key)
        {
            return GetSingleRelated(key, p => p.Company);
        }

        [Produces(typeof(Site))]
        [HttpPost]
        public IActionResult Post([FromBody]Site item)
        {
            return PostEntity(item);
        }

        [Produces(typeof(Site))]
        public IActionResult Patch(int key, Delta<Site> delta)
        {
            return PatchEntity(key, delta);
        }

        [Produces(typeof(void))]
        public IActionResult Delete(int key)
        {
            return DeleteEntity(key);
        }
    }
}
