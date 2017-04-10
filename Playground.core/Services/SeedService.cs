using Playground.core.Models;
using System.Linq;

namespace Playground.core.Services
{
    public class SeedService
    {
        private readonly PlaygroundContext m_context;

        public SeedService( PlaygroundContext context )
        {
            m_context = context;
        }
        
        public void Seed() 
        {
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

            m_context.SaveChanges();
        }
    }
}
