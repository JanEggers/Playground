using System.Data.Entity;
namespace Playground.Models
{
    public class PlaygroundContext : DbContext
    {
        public PlaygroundContext() 
        {
        }

        public IDbSet<Company> Companies { get; set; }

        public IDbSet<Site> Sites { get; set; }
    }
}