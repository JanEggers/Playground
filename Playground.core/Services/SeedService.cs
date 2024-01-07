namespace Playground.core.Services;

public class SeedService
{
    private readonly PlaygroundContext m_context;

    public SeedService( PlaygroundContext context )
    {
        m_context = context;
    }
    
    public async Task SeedAsync() 
    {
        await m_context.Database.MigrateAsync();

        var company = m_context.Companies.FirstOrDefault(p => p.Name == "Hallo");

        if (company == null)
        {
            company = new Company()
            {
                Name = "Hallo"
            };

            m_context.Companies.Add(company);
        }

        if (!m_context.Sites.Any(p => p.Name == "Hallo"))
        {
            m_context.Sites.Add(new Site()
            {
                Name = "Hallo",
                Company = company,
            });
        }

        if (!m_context.Sites.Any(p => p.Name == "Bye"))
        {
            m_context.Sites.Add(new Site()
            {
                Name = "Bye",
                Company = company,
            });
        }

        var missing = 100000 - m_context.Companies.Count();

        for (int i = 0; i < missing; i++)
        {

            m_context.Companies.Add(new Company()
            {
                Name = $"C{i}"
            });
        }
        
        await m_context.SaveChangesAsync();

        //var memCompanies = m_context.CompaniesMem.ToDictionary(p => p.Name);

        //foreach (var company2 in m_context.Companies)
        //{
        //    if (!memCompanies.TryGetValue(company2.Name, out _))
        //    {
        //        m_context.CompaniesMem.Add(new CompanyMem()
        //        {
        //            Name = company2.Name,
        //        });
        //    }
        //}


        //await m_context.SaveChangesAsync();


        //foreach (var companyx in m_context.Companies)
        //{
        //    var missing = 1000 - company.Sites.Count;

        //    for (int i = 0; i < missing; i++)
        //    {
        //        m_context.Sites.Add(new Site()
        //        {
        //            Name = $"S{i}",
        //            Company = companyx
        //        });
        //    }
        //}

        //m_context.SaveChanges();
    }
}
