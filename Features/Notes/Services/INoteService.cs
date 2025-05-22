using SuperInvestor.Features.Notes.Data;

namespace SuperInvestor.Features.Notes.Services;

public interface INoteService
{
    event EventHandler<string> NotesChanged;
    Task<bool> HasNotesForFiling(string userId, string ticker, string accessionNumber);
    Task<Note> GetNote(Guid noteId);

    Task<Note> AddNote(
        string userId,
        string ticker,
        string accessionNumber,
        string text,
        int startIndex,
        int endIndex);

    Task<bool> UpdateNote(
        Guid noteId,
        string text);

    Task<bool> DeleteNote(
        Guid noteId);

    Task<IEnumerable<Note>> GetNotes(string userId);
    Task<IEnumerable<Note>> GetNotes(string userId, string ticker);
    Task<IEnumerable<Note>> GetNotes(string userId, string ticker, string accessionNumber);
}