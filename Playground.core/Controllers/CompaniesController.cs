using System;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

using Playground.core.Models;
using AutoMapper.QueryableExtensions;
using System.Linq;
using System.Collections.Generic;

namespace Playground.core.Controllers
{
    [Authorize]
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

        [HttpGet(nameof(GetViewModels))]
        public IActionResult GetViewModels()
        {
            var vms = (from c in m_db.Companies
                       join s in m_db.Sites on c.Id equals s.CompanyId
                       join t in m_db.Translations on c.Id equals t.Id
                       select new CompanySite
                       {
                           Company = c,
                           Site = s,
                           Translation = t
                       }).ProjectTo<SiteViewModel>().Where(p => p.SiteName == "Hallo").ToList();

            return Ok(vms);
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
