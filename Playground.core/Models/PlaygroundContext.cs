
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
            //Database.Migrate();

            //var translations = new List<Translation>();

            //for (int i = 0; i < 100; i++)
            //{
            //    translations.Add(new Translation()
            //    {
            //        Id = i,
            //        Text = i.ToString()
            //    });
            //}

            //Translations = translations.AsQueryable();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Company>()
                .ForSqlServerToTable("Companies");

            builder.Entity<CompanySub>()
                .HasBaseType<Company>();

            builder.Entity<CompanySub2>()
                .HasBaseType<Company>();

            base.OnModelCreating(builder);
        }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Site> Sites { get; set; }

        public IQueryable<Translation> Translations { get; set; }
    }
}