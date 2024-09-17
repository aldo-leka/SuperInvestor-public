namespace SuperInvestor.Services;

public class FilingEventService
{
    public event Action<int, int, string> TextSelected;
    public event Action TextUnselected;
    public event Action<string> MobileNoteMenuOpened;
    public event Action<int, int, string> MobileTextSelected;

    public void OnTextSelected(int startIndex, int endIndex, string selectedText)
    {
        TextSelected?.Invoke(startIndex, endIndex, selectedText);
    }

    public void OnTextUnselected()
    {
        TextUnselected?.Invoke();
    }

    public void OnMobileNoteMenuOpened(string noteId)
    {
        MobileNoteMenuOpened?.Invoke(noteId);
    }

    public void OnMobileTextSelected(int startIndex, int endIndex, string selectedText)
    {
        MobileTextSelected?.Invoke(startIndex, endIndex, selectedText);
    }
}