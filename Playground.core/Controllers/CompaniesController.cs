using System;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Playground.core.Models;
using AutoMapper.QueryableExtensions;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.OData;
using AutoMapper;

namespace Playground.core.Controllers
{
    [Authorize]
    [EnableQuery]
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

        /// <summary>
        /// Companies?$filter=Name eq 'C700'
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IQueryable<Company>> Get()
        {
            return GetAll();
        }

        [HttpGet(nameof(GetViewModels))]
        public ActionResult<IQueryable<SiteViewModel>> GetViewModels()
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

        public ActionResult<Company> Get(int key)
        {
            return GetSingleEntity(key);
        }
        
        public ActionResult<IQueryable<Site>> GetSites(int key)
        {
            return GetManyRelated(key, p => p.Sites);
        }

        [Produces(typeof(Company))]
        [HttpPost]
        public IActionResult Post([FromBody]Company item)
        {
            return PostEntity(item);
        }

        [Produces(typeof(Company))]
        public IActionResult Patch(int key, Delta<Company> delta)
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
