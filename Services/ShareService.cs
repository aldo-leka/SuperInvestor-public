using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SuperInvestor.Data;

namespace SuperInvestor.Services;

public class ShareService(ResearchService researchService, NoteService noteService, NavigationManager navigationManager)
{
    private readonly ResearchService _researchService = researchService;
    private readonly NoteService _noteService = noteService;
    private readonly NavigationManager _navigationManager = navigationManager;

    public async Task<string> GenerateShareLinkForResearch(string userId, string ticker, string accessionNumber)
    {
        var existingResearch = await _researchService.GetResearch(userId, ticker, accessionNumber);
        if (existingResearch == null)
        {
            var notes = await (await _noteService.GetNotes(userId, ticker, accessionNumber))
                .ToListAsync();

            if (notes.Any())
            {
                var research = await _researchService.AddResearch(userId, ticker, accessionNumber);
                return _navigationManager.BaseUri + $"r/{research.ShortId}";
            }
            else
            {
                return null; // No notes to share
            }
        }
        else
        {
            return _navigationManager.BaseUri + $"r/{existingResearch.ShortId}";
        }
    }

    public async Task<string> GenerateShareLinkForNote(Guid noteId)
    {
        var note = await _noteService.GetNote(noteId);
        if (note != null)
        {
            var research = await _researchService.GetResearch(note.UserId, note.Ticker, note.AccessionNumber);
            research ??= await _researchService.AddResearch(note.UserId, note.Ticker, note.AccessionNumber);

            return _navigationManager.BaseUri + $"r/{research.ShortId}/n/{note.ShortId}";
        }

        return null;
    }
}