namespace SuperInvestor.Models;

public class Filing
{
    public string Cik { get; set; }
    public string AccessionNumber { get; set; }
    public string FilingDate { get; set; }
    public string Form { get; set; }
    public string PrimaryDocument { get; set; }
    public Uri Url { get; set; }
    public bool HasNotes { get; set; }
    public string Category { get; set; }
}