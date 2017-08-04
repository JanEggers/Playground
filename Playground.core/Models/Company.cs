using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Playground.core.Models
{
    public class Company 
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Site> Sites { get; set; }
    }

    public class CompanySub : Company
    {
        public int Sub { get; set; }
    }

    public class CompanySub2 : Company
    {
        public int Sub2 { get; set; }
    }
}