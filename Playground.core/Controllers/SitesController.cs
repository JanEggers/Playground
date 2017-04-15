using System.Linq.Expressions;
using System;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

using Playground.core.Models;
using System.Collections.Generic;

namespace Playground.core.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
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
        [HttpGet("{key}", Name = nameof(GetSingleSite))]
        public IActionResult GetSingleSite([FromQuery] int key)
        {
            return GetSingleEntity( key );
        }

        [Produces(typeof(Site))]
        [HttpPost]
        public IActionResult Post([FromBody]Site item)
        {
            return PostEntity(item, nameof(GetSingleSite));
        }

        [Produces(typeof(Site))]
        [HttpPatch("{key}")]
        public IActionResult Patch([FromQuery] int key, [FromBody]JsonPatchDocument<Site> patch)
        {
            return PatchEntity(key, patch);
        }

        [Produces(typeof(void))]
        [HttpDelete("{key}")]
        public IActionResult Delete([FromQuery] int key)
        {
            return DeleteEntity(key);
        }
    }
}
