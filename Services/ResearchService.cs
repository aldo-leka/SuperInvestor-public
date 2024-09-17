using Microsoft.EntityFrameworkCore;
using SuperInvestor.Data;

namespace SuperInvestor.Services;

public class ResearchService(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task<Research> GetResearch(string shortId)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _dbContext.Researches
                .FirstOrDefaultAsync(r => r.ShortId == shortId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Research> GetResearch(string userId, string ticker, string accessionNumber)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _dbContext.Researches
                .FirstOrDefaultAsync(r => r.UserId == userId
                    && r.Ticker == ticker
                    && r.AccessionNumber == accessionNumber);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Research> AddResearch(
        string userId,
        string ticker,
        string accessionNumber)
    {
        await _semaphore.WaitAsync();
        try
        {
            var research = new Research
            {
                UserId = userId,
                Ticker = ticker,
                AccessionNumber = accessionNumber
            };

            _dbContext.Researches.Add(research);
            await _dbContext.SaveChangesAsync();

            return research;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
