using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Playground.core.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Playground.Core.Test
{
    public class AutoMapperTests
    {
        [Fact]
        public void Test()
        {
            using var services = new ServiceCollection()
                .AddDbContext<PlaygroundContext>(o => {
                    o.UseInMemoryDatabase( nameof( PlaygroundContext ) );
                })
                .AddMappings()
                .BuildServiceProvider();

            using var context = services.GetService<PlaygroundContext>();
            var mapping = services.GetService<IConfigurationProvider>();

            context.Companies.Add(new Company()
            {
                Name = "A"
            });

            context.SaveChanges();
            var companyQuery = context.Companies
                .ProjectTo<CompanyViewModel>(mapping);

            var companies = companyQuery.ToList();

            context.Companies.Add(new Company()
            {
                Name = "B"
            });

            context.SaveChanges();

            companies[0].Name = "x";

            companyQuery.ProjectInto(companies, mapping);

            context.Companies.Remove(context.Companies.First());

            context.SaveChanges();

            companyQuery.ProjectInto(companies, mapping);
        }
    }

    public static class ProjectionExtensions
    {
        public static void ProjectInto<T>(this IQueryable<T> query, IList<T> target, IConfigurationProvider configurationProvider)
        {            
            var mapper = configurationProvider.CreateMapper();
            using var enumerator = query.GetEnumerator();
            for (int i = 0; i < target.Count; i++)
            {
                if (enumerator.MoveNext())
                {
                    mapper.Map(enumerator.Current, target[i]);
                }
                else
                {
                    target.RemoveAt(i);
                    i--;
                }
            }

            while (enumerator.MoveNext())
            {
                target.Add(enumerator.Current);
            }
        }
    }
}
