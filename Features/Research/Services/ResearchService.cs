using Microsoft.EntityFrameworkCore;
using SuperInvestor.Features.Common.Data;
using SuperInvestor.Features.Research.Data;

namespace SuperInvestor.Features.Research.Services;

public class ResearchService(IDbContextFactory<ApplicationDbContext> factory)
{
    public async Task<SuperInvestor.Features.Research.Data.Research> GetResearch(string shortId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Researches
            .FirstOrDefaultAsync(r => r.ShortId == shortId);
    }

    public async Task<SuperInvestor.Features.Research.Data.Research> GetResearch(string userId, string ticker, string accessionNumber)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Researches
            .FirstOrDefaultAsync(r => r.UserId == userId
                && r.Ticker == ticker
                && r.AccessionNumber == accessionNumber);
    }

    public async Task<SuperInvestor.Features.Research.Data.Research> AddResearch(
        string userId,
        string ticker,
        string accessionNumber)
    {
        var research = new SuperInvestor.Features.Research.Data.Research
        {
            UserId = userId,
            Ticker = ticker,
            AccessionNumber = accessionNumber
        };

        await using var db = await factory.CreateDbContextAsync();
        db.Researches.Add(research);
        await db.SaveChangesAsync();

        return research;
    }
}
