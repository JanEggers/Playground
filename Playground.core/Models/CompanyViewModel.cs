using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Playground.core.Models
{
    public class CompanyViewModel
    {
        static int count;

        public CompanyViewModel()
        {
            count++;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int NumberOfSites { get; set; }
    }
}
