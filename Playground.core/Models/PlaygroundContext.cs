
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Playground.core.Models
{
    public class PlaygroundContext : IdentityDbContext<PlaygroundUser>
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