namespace SuperInvestor.Features.Notes.Services;

public interface INoteHighlightService
{
    event Func<int, int, string, Task> HighlightRequested;
    event Action FilingContentReady;
    event Func<int, int, string, Task> HoverHighlightRequested;
    event Func<string, Task> ClearHoverHighlightRequested;
    Task RequestHighlight(int startIndex, int endIndex, string noteId);
    Task RequestHoverHighlight(int startIndex, int endIndex, string noteId);
    Task RequestClearHoverHighlight(string noteId);
    void NotifyFilingContentReady();
}