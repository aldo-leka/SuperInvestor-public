namespace SuperInvestor.Features.Research.Services;

public interface IShareService
{
    Task<string> GenerateShareLinkForResearch(string userId, string ticker, string accessionNumber);
    Task<string> GenerateShareLinkForNote(Guid noteId);
}