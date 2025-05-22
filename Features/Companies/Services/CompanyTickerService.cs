using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using SuperInvestor.Features.Companies.Models;

namespace SuperInvestor.Features.Companies.Services;

public class CompanyTickerService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache) : ICompanyTickerService
{
    private const string CacheKey = "CompanyTickers";

    public async Task<IEnumerable<Ticker>> GetTickersAsync()
    {
        if (memoryCache.TryGetValue(CacheKey, out IEnumerable<Ticker> tickers)) return tickers;
        tickers = await FetchTickersFromApiAsync();
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(1));
        memoryCache.Set(CacheKey, tickers, cacheEntryOptions);
        return tickers;
    }

    private async Task<IEnumerable<Ticker>> FetchTickersFromApiAsync()
    {
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent, "Lekasoft aldo@lekasoft.com");
        var response = await client.GetFromJsonAsync<Dictionary<string, Ticker>>("https://www.sec.gov/files/company_tickers.json");
        return response.Values.AsEnumerable();
    }

    public async Task<Ticker> GetTickerBySymbolAsync(string symbol)
    {
        var tickers = await GetTickersAsync();
        return tickers.FirstOrDefault(t => t.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Submission> GetSubmission(string cik)
    {
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent, "Lekasoft aldo@lekasoft.com");

        var paddedCik = cik.PadLeft(10, '0');
        var url = $"https://data.sec.gov/submissions/CIK{paddedCik}.json";

        try
        {
            var submission = await client.GetFromJsonAsync<Submission>(url);
            return submission;
        }
        catch (HttpRequestException e)
        {
            // Log the exception or handle it as appropriate for your application
            Console.WriteLine($"Error fetching submission data: {e.Message}");
            return null;
        }
    }
}
