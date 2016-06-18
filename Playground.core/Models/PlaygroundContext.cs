
using Microsoft.EntityFrameworkCore;
using OpenIddict;

namespace Playground.core.Models
{
    public class PlaygroundContext : OpenIddictContext<PlaygroundUser>
    {
        public PlaygroundContext(DbContextOptions<PlaygroundContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Site> Sites { get; set; }
    }
}