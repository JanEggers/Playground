using Azure;
using Microsoft.AspNetCore.Http;
using Playground.core.Models;
using System.Buffers;
using System.Text;

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

    [Produces(typeof(IEnumerable<Company>))]
    [HttpGet]
    public async Task GetAll([FromQuery] int take)
    {
        var companies = _context.Companies.Take(take).AsNoTracking()
        .Select(c => "{\"Id\":" + c.Id +",\"Name\":\""+c.Name+"\"}");

        var writer = Response.BodyWriter;
        var encoding = Encoding.UTF8;

        encoding.GetBytes("[", writer);
        var sep = string.Empty;
        foreach (var company in companies)
        {
            encoding.GetBytes(sep, writer);
            encoding.GetBytes(company, writer);
            sep = ",";
        }
        encoding.GetBytes("]", writer);

        await writer.FlushAsync();
    }

    [HttpGet("conventional")]
    public async Task<IEnumerable<Company>> GetAllConventional([FromQuery] int take)
    {
        return _context.Companies.Take(take).AsNoTracking();
    }
}
