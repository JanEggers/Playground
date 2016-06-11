using System;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

using Playground.core.Models;

namespace Playground.core.Controllers
{
    [Route("api/[controller]")]
    public class CompaniesController : EntityController<Company,int>
    {
        public CompaniesController(PlaygroundContext ctx)
            : base(ctx, ctx.Companies)
        {
        }

        protected override Expression<Func<Company, bool>> Find(int key)
        {
            return p => p.Id == key;
        }

        protected override int GetKey(Company item)
        {
            return item.Id;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return GetAll();
        }

        [HttpGet("{key}", Name = nameof(GetSingleCompany))]
        public IActionResult GetSingleCompany([FromQuery] int key)
        {
            return GetSingleEntity(key);
        }

        [HttpPost]
        public IActionResult Post([FromBody]Company item)
        {
            return PostEntity(item,nameof(GetSingleCompany));
        }

        [HttpPatch("{key}")]
        public IActionResult Patch([FromQuery] int key, [FromBody]JsonPatchDocument<Company> patch)
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
