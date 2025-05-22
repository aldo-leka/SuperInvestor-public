using Microsoft.AspNetCore.Mvc;
using SuperInvestor.Features.Companies.Services;

namespace SuperInvestor.Features.Companies.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TickersController(ICompanyTickerService tickers, ILogger<TickersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTickers([FromQuery] string query)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 1)
        {
            return Ok(Array.Empty<string>());
        }

        try
        {
            logger.LogInformation("Fetching tickers with query: {Query}", query);
            var tickers1 = await tickers.GetTickersAsync();
            
            var filteredTickers = tickers1
                .Where(t => t.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                            t.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .Select(t => $"{t.Symbol} ({t.Name})")
                .ToArray();
            
            logger.LogInformation("Successfully retrieved {Count} tickers for query: {Query}", filteredTickers.Length, query);
            return Ok(filteredTickers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching tickers for query: {Query}", query);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}