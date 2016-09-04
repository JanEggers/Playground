
using Microsoft.EntityFrameworkCore;
using OpenIddict;

namespace Playground.core.Models
{
    public class PlaygroundContext : OpenIddictDbContext<PlaygroundUser>
    {
        public PlaygroundContext(DbContextOptions<PlaygroundContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Site> Sites { get; set; }
    }
}