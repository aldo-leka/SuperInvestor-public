namespace SuperInvestor.Features.Companies.Services;

public interface IFilingEventService
{
    event Action<int, int, string> TextSelected;
    event Action TextUnselected;
    event Action<string> MobileNoteMenuOpened;
    event Action<int, int, string> MobileTextSelected;
    void OnTextSelected(int startIndex, int endIndex, string selectedText);
    void OnTextUnselected();
    void OnMobileNoteMenuOpened(string noteId);
    void OnMobileTextSelected(int startIndex, int endIndex, string selectedText);
}