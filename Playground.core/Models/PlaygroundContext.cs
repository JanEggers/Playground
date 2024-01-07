
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Playground.core.Models
{
    public class PlaygroundContext :  IdentityDbContext<PlaygroundUser>
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
            builder.UseOpenIddict();

            builder.Entity<Company>()
                .ToTable( "Companies" );


            //builder.Entity<CompanyMem>()
            //    .ToTable("CompaniesInMemory", p => p.IsMemoryOptimized());

            builder.Entity<CompanySub>()
                .HasBaseType<Company>();

            builder.Entity<CompanySub2>()
                .HasBaseType<Company>();

            base.OnModelCreating(builder);
        }

        public DbSet<Company> Companies { get; set; }
        //public DbSet<CompanyMem> CompaniesMem { get; set; }

        public DbSet<Site> Sites { get; set; }

        public IQueryable<Translation> Translations { get; set; }
    }

    //public class CompanyMem
    //{
    //    [Key]
    //    public int Id { get; set; }

    //    public string Name { get; set; }

    //    public virtual ICollection<Site> Sites { get; set; }
    //}
}