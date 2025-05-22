namespace SuperInvestor.Features.Research.Services;

public interface IResearchService
{
    Task<Data.Research> GetResearch(string shortId);
    Task<Data.Research> GetResearch(string userId, string ticker, string accessionNumber);

    Task<Data.Research> AddResearch(
        string userId,
        string ticker,
        string accessionNumber);
}