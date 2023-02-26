using Microsoft.Extensions.Configuration;
using Playground.core.Extensions;
using Respawn;
using Respawn.Graph;
using System;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace Playground.Core.Test
{
    public class AutoMapperTests
    {
        [Fact]
        public async Task Test()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] {
                    new KeyValuePair<string, string>("ConnectionStrings:PlaygroundContext", "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=PlaygroundTest;Integrated Security=SSPI")
                })
                .Build();

            using var services = new ServiceCollection()
                .AddPlayground(config)
                .BuildServiceProvider();

            using var context = services.GetService<PlaygroundContext>();
            var mapping = services.GetService<IConfigurationProvider>();

            await ClearDb(services);

            await context.Database.MigrateAsync();

            context.Companies.Add(new Company()
            {
                Name = "A"
            });

            await context.SaveChangesAsync();
            var companyQuery = context.Companies
                .ProjectTo<CompanyViewModel>(mapping);

            var companies = companyQuery.ToList();

            context.Companies.Add(new Company()
            {
                Name = "B"
            });

            await context.SaveChangesAsync();

            companies[0].Name = "x";

            companyQuery.ProjectInto(companies, mapping);

            context.Companies.Remove(context.Companies.First());

            await context.SaveChangesAsync();

            companyQuery.ProjectInto(companies, mapping);
        }

        private async Task ClearDb(IServiceProvider serviceProvider) 
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlaygroundContext>();

            using (var conn = context.Database.GetDbConnection())
            {
                try
                {
                    await conn.OpenAsync();
                }
                catch (System.Exception)
                {
                    // ignore
                    return;
                }

                var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
                {
                    TablesToIgnore = new Table[]
                        {
                            "__EFMigrationsHistory",
                        }
                });

                await respawner.ResetAsync(conn);
            }
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
