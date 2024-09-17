namespace SuperInvestor.Models;

public class NoteViewModel
{
    public string Id { get; set; }
    public string ShortId { get; set; }
    public string InitialText { get; set; }
    public string AdditionalText { get; set; }
    public bool ShowMore { get; set; }
    public DateTime Date { get; set; }
    public bool Edit { get; set; }
    public string EditedText { get; set; }
    public bool AskDelete { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public int InitialNoteLength { get; set; }

    public string FullText
    {
        get
        {
            return InitialText + AdditionalText;
        }
        set
        {
            ResetTextFields(InitialNoteLength);
            var maxLength = value.Length > InitialNoteLength ? InitialNoteLength : value.Length;
            InitialText = value.Substring(0, maxLength);
            AdditionalText = value.Length > InitialNoteLength
                ? value.Substring(maxLength, value.Length - maxLength)
                : "";
        }
    }

    public void ResetTextFields(int newInitialNoteLength)
    {
        InitialNoteLength = newInitialNoteLength;
        var fullText = FullText;
        InitialText = fullText.Substring(0, Math.Min(fullText.Length, InitialNoteLength));
        AdditionalText = fullText.Length > InitialNoteLength
            ? fullText.Substring(InitialNoteLength)
            : "";
    }
}