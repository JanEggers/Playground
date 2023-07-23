using HotChocolate.Data;
using HotChocolate.Types;
using System.Runtime;

namespace Playground.core.GraphQL;

public class Query
{
    [UseOffsetPaging(MaxPageSize =10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Company> GetCompanies(PlaygroundContext context) 
    {
        return context.Companies.AsNoTracking();
    }
}
