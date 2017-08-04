using System;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

using Playground.core.Models;
using AutoMapper.QueryableExtensions;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;

namespace Playground.core.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class CompaniesController : EntityController<Company,int>
    {
        private IConfigurationProvider m_mapperConfig;

        public CompaniesController(PlaygroundContext ctx, IConfigurationProvider mapperConfig)
            : base(ctx, ctx.Companies)
        {
            m_mapperConfig = mapperConfig;
        }

        protected override Expression<Func<Company, bool>> Find(int key)
        {
            return p => p.Id == key;
        }

        protected override int GetKey(Company item)
        {
            return item.Id;
        }

        [Produces(typeof(IEnumerable<Company>))]
        [HttpGet]
        public IActionResult Get()
        {
            return GetAll();
        }

        [Produces(typeof(IEnumerable<SiteViewModel>))]
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
                       }).ProjectTo<SiteViewModel>(m_mapperConfig).Where(p => p.SiteName == "Hallo").ToList();

            return Ok(vms);
        }

        [Produces(typeof(Company))]
        [HttpGet("{key}", Name = nameof(GetSingleCompany))]
        public IActionResult GetSingleCompany(int key)
        {
            return GetSingleEntity(key);
        }
        
        [Produces(typeof(IEnumerable<Site>))]
        [HttpGet("{key}/Sites", Name = nameof(GetSitesOfCompany))]
        public IActionResult GetSitesOfCompany(int key)
        {
            return GetManyRelated(key, p => p.Sites);
        }

        [Produces(typeof(Company))]
        [HttpPost]
        public IActionResult Post([FromBody]Company item)
        {
            return PostEntity(item,nameof(GetSingleCompany));
        }

        [Produces(typeof(Company))]
        [HttpPatch("{key}")]
        public IActionResult Patch([FromQuery] int key, [FromBody]JsonPatchDocument<Company> patch)
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
