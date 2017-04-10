using System.Linq.Expressions;
using System;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

using Playground.core.Models;

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

        [HttpGet]
        public IActionResult Get()
        {
            return GetAll();
        }
        
        [HttpGet("{key}", Name = nameof(GetSingleSite))]
        public IActionResult GetSingleSite([FromQuery] int key)
        {
            return GetSingleEntity( key );
        }

        [HttpPost]
        public IActionResult Post([FromBody]Site item)
        {
            return PostEntity(item, nameof(GetSingleSite));
        }

        [HttpPatch("{key}")]
        public IActionResult Patch([FromQuery] int key, [FromBody]JsonPatchDocument<Site> patch)
        {
            return PatchEntity(key, patch);
        }

        [HttpDelete("{key}")]
        public IActionResult Delete([FromQuery] int key)
        {
            return DeleteEntity(key);
        }
    }
}
