using Microsoft.AspNetCore.Mvc;
using SuperInvestor.Services;

namespace SuperInvestor.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TickersController(CompanyTickerService tickers, ILogger<TickersController> logger) : ControllerBase
{
    private readonly ILogger<TickersController> _logger = logger;
    private readonly CompanyTickerService _tickers = tickers;

    [HttpGet]
    public async Task<IActionResult> GetTickers([FromQuery] string query)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 1)
        {
            return Ok(Array.Empty<string>());
        }

        try
        {
            _logger.LogInformation("Fetching tickers with query: {Query}", query);
            var tickers = await _tickers.GetTickersAsync();
            
            var filteredTickers = tickers
                .Where(t => t.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                            t.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .Select(t => $"{t.Symbol} ({t.Name})")
                .ToArray();
            
            _logger.LogInformation("Successfully retrieved {Count} tickers for query: {Query}", filteredTickers.Length, query);
            return Ok(filteredTickers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tickers for query: {Query}", query);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}