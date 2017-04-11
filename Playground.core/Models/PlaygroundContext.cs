
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Playground.core.Models
{
    public class PlaygroundContext : IdentityDbContext<PlaygroundUser>
    {
        public PlaygroundContext(DbContextOptions<PlaygroundContext> options)
            : base(options)
        {
            Database.Migrate();

            var translations = new List<Translation>();

            for (int i = 0; i < 100; i++)
            {
                translations.Add(new Translation()
                {
                    Id = i,
                    Text = i.ToString()
                });
            }

            Translations = translations.AsQueryable();
        }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Site> Sites { get; set; }

        public IQueryable<Translation> Translations { get; set; }
    }
}