using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Playground.core.Models
{
    public class CompanySite
    {
        public Company Company { get; set; }

        public Site Site { get; set; }
    }

    public class SiteViewModel
    {
        public string CompanyName { get; set; }

        public string SiteName { get; set; }
    }
}
