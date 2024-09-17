using Microsoft.EntityFrameworkCore;
using SuperInvestor.Data;

namespace SuperInvestor.Services;

public class NoteService(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public event EventHandler<string> NotesChanged;

    private void OnNotesChanged(string accessionNumber)
    {
        NotesChanged?.Invoke(this, accessionNumber);
    }

    public async Task<bool> HasNotesForFiling(string userId, string ticker, string accessionNumber)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _dbContext.Notes
                .AnyAsync(n => n.UserId == userId
                    && n.Ticker == ticker.ToUpper()
                    && n.AccessionNumber == accessionNumber);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Note> GetNote(Guid noteId)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _dbContext.Notes.FindAsync(noteId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Note> AddNote(
        string userId,
        string ticker,
        string accessionNumber,
        string text,
        int startIndex,
        int endIndex)
    {
        await _semaphore.WaitAsync();
        try
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

            _dbContext.Notes.Add(newNote);
            await _dbContext.SaveChangesAsync();

            OnNotesChanged(accessionNumber);

            return newNote;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UpdateNote(
        Guid noteId,
        string text)
    {
        await _semaphore.WaitAsync();
        try
        {
            var note = await _dbContext.Notes.FindAsync(noteId);
            if (note != null)
            {
                note.Text = text;
                note.UpdatedAt = DateTime.UtcNow;
                return await _dbContext.SaveChangesAsync() > 0;
            }

            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> DeleteNote(
        Guid noteId)
    {
        await _semaphore.WaitAsync();
        try
        {
            var note = await _dbContext.Notes.FindAsync(noteId);
            if (note != null)
            {
                _dbContext.Notes.Remove(note);
                await _dbContext.SaveChangesAsync();
                OnNotesChanged(note.AccessionNumber);
                return true;
            }

            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IQueryable<Note>> GetNotes(string userId)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _dbContext.Notes.Where(n => n.UserId == userId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IQueryable<Note>> GetNotes(string userId, string ticker)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _dbContext.Notes
                .Where(n => n.UserId == userId
                    && n.Ticker == ticker.ToUpper());
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IQueryable<Note>> GetNotes(string userId, string ticker, string accessionNumber)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _dbContext.Notes
                .Where(n => n.UserId == userId
                    && n.Ticker == ticker.ToUpper()
                    && n.AccessionNumber == accessionNumber);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}