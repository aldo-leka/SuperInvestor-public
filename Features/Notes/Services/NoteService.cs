using Microsoft.EntityFrameworkCore;
using SuperInvestor.Features.Common.Data;
using SuperInvestor.Features.Notes.Data;

namespace SuperInvestor.Features.Notes.Services;

public class NoteService(IDbContextFactory<ApplicationDbContext> factory)
{
    public event EventHandler<string> NotesChanged;

    private void OnNotesChanged(string accessionNumber)
    {
        NotesChanged?.Invoke(this, accessionNumber);
    }

    public async Task<bool> HasNotesForFiling(string userId, string ticker, string accessionNumber)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Notes
            .AnyAsync(n => n.UserId == userId
                && n.Ticker == ticker.ToUpper()
                && n.AccessionNumber == accessionNumber);
    }

    public async Task<Note> GetNote(Guid noteId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Notes.FindAsync(noteId);
    }

    public async Task<Note> AddNote(
        string userId,
        string ticker,
        string accessionNumber,
        string text,
        int startIndex,
        int endIndex)
    {
        var newNote = new Note
        {
            UserId = userId,
            Ticker = ticker.ToUpper(),
            AccessionNumber = accessionNumber,
            Text = text,
            StartIndex = startIndex,
            EndIndex = endIndex,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await using var db = await factory.CreateDbContextAsync();

        db.Notes.Add(newNote);
        await db.SaveChangesAsync();

        OnNotesChanged(accessionNumber);

        return newNote;
    }

    public async Task<bool> UpdateNote(
        Guid noteId,
        string text)
    {
        await using var db = await factory.CreateDbContextAsync();
        var note = await db.Notes.FindAsync(noteId);
        if (note != null)
        {
            note.Text = text;
            note.UpdatedAt = DateTime.UtcNow;
            return await db.SaveChangesAsync() > 0;
        }

        return false;
    }

    public async Task<bool> DeleteNote(
        Guid noteId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var note = await db.Notes.FindAsync(noteId);
        if (note != null)
        {
            db.Notes.Remove(note);
            await db.SaveChangesAsync();
            OnNotesChanged(note.AccessionNumber);
            return true;
        }

        return false;
    }

    public async Task<IEnumerable<Note>> GetNotes(string userId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Notes.Where(n => n.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<Note>> GetNotes(string userId, string ticker)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Notes
            .Where(n => n.UserId == userId
                && n.Ticker == ticker.ToUpper())
            .ToListAsync();
    }

    public async Task<IEnumerable<Note>> GetNotes(string userId, string ticker, string accessionNumber)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Notes
            .Where(n => n.UserId == userId
                && n.Ticker == ticker.ToUpper()
                && n.AccessionNumber == accessionNumber)
            .ToListAsync();
    }
}