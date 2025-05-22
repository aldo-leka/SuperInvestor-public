using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SuperInvestor.Features.Notes.Services;
using SuperInvestor.Features.Research.Services;

namespace SuperInvestor.Features.Research.Services;

public class ShareService(IResearchService researchService, INoteService noteService, NavigationManager navigationManager) : IShareService
{
    public async Task<string> GenerateShareLinkForResearch(string userId, string ticker, string accessionNumber)
    {
        var existingResearch = await researchService.GetResearch(userId, ticker, accessionNumber);
        if (existingResearch == null)
        {
            var notes = await noteService.GetNotes(userId, ticker, accessionNumber);

            if (notes.Any())
            {
                var research = await researchService.AddResearch(userId, ticker, accessionNumber);
                return navigationManager.BaseUri + $"r/{research.ShortId}";
            }
            else
            {
                return null; // No notes to share
            }
        }
        else
        {
            return navigationManager.BaseUri + $"r/{existingResearch.ShortId}";
        }
    }

    public async Task<string> GenerateShareLinkForNote(Guid noteId)
    {
        var note = await noteService.GetNote(noteId);
        if (note == null) return null;
        var research = await researchService.GetResearch(note.UserId, note.Ticker, note.AccessionNumber);
        research ??= await researchService.AddResearch(note.UserId, note.Ticker, note.AccessionNumber);

        return navigationManager.BaseUri + $"r/{research.ShortId}/n/{note.ShortId}";

    }
}