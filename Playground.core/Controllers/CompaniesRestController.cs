using Playground.core.Models;

namespace Playground.core.Controllers;

[ApiController]
[Route("[controller]")]
public class CompaniesRestController : ControllerBase
{
    private readonly PlaygroundContext _context;

    public CompaniesRestController(PlaygroundContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IAsyncEnumerable<Company> GetAll([FromQuery] int take) 
    {
        return _context.Companies.Take(take).AsNoTracking().AsAsyncEnumerable();
    }
}
