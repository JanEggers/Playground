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
}