using SuperInvestor.Features.Companies.Models;

namespace SuperInvestor.Features.Companies.Services;

public interface ICompanyTickerService
{
    Task<IEnumerable<Ticker>> GetTickersAsync();
    Task<Ticker> GetTickerBySymbolAsync(string symbol);
    Task<Submission> GetSubmission(string cik);
}