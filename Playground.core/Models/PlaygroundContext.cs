
using Microsoft.EntityFrameworkCore;

namespace Playground.core.Models
{
    public class PlaygroundContext : DbContext
    {
        public PlaygroundContext(DbContextOptions<PlaygroundContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Site> Sites { get; set; }
    }
}