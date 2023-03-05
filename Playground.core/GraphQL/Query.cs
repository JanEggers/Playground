using HotChocolate.Data;

namespace Playground.core.GraphQL;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Company> GetCompanies(PlaygroundContext context) 
    {
        return context.Companies;
    }
}
