namespace SuperInvestor.Services;

public class NoteHighlightService
{
    public event Func<int, int, string, Task> HighlightRequested;
    public event Action FilingContentReady;
    public event Func<int, int, string, Task> HoverHighlightRequested;
    public event Func<string, Task> ClearHoverHighlightRequested;

    public async Task RequestHighlight(int startIndex, int endIndex, string noteId)
    {
        if (HighlightRequested != null)
        {
            await HighlightRequested.Invoke(startIndex, endIndex, noteId);
        }
    }

    public async Task RequestHoverHighlight(int startIndex, int endIndex, string noteId)
    {
        if (HoverHighlightRequested != null)
        {
            await HoverHighlightRequested.Invoke(startIndex, endIndex, noteId);
        }
    }

    public async Task RequestClearHoverHighlight(string noteId)
    {
        if (ClearHoverHighlightRequested != null)
        {
            await ClearHoverHighlightRequested.Invoke(noteId);
        }
    }

    public void NotifyFilingContentReady()
    {
        FilingContentReady?.Invoke();
    }
}