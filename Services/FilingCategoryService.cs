using SuperInvestor.Models;

namespace SuperInvestor.Services;

public class FilingCategoryService
{
    public List<Filing> CategorizeFilings(Submission submission)
    {
        var categorizedFilings = new List<Filing>();

        for (int i = 0; i < submission.Filings.Recent.AccessionNumber.Length; i++)
        {
            if (!submission.Filings.Recent.PrimaryDocument[i].EndsWith(".pdf"))
            {
                var form = submission.Filings.Recent.Form[i];
                var filing = new Filing
                {
                    Cik = submission.Cik,
                    AccessionNumber = submission.Filings.Recent.AccessionNumber[i],
                    FilingDate = submission.Filings.Recent.FilingDate[i],
                    Form = form,
                    PrimaryDocument = submission.Filings.Recent.PrimaryDocument[i],
                    Category = GetFilingCategory(form)
                };

                categorizedFilings.Add(filing);
            }
        }

        return categorizedFilings;
    }

    public string GetFilingCategory(string form)
    {
        return form switch
        {
            "10-Q" or "10-Q/A" or "10-K" or "10-K/A" or "10-K405" or "20-F" or "20-F/A" => "Financials",
            "S-8" or "S-8 POS" or "F-6EF" or "FWP" or "424B2" or "424B3" or "424B5" or "F-3ASR" or "F-4" or "F-4/A" or "8-A12B" or "F-1/A" or "F-6" or "DRS" or "F-1" or "S-3ASR" or "SC TO-I/A" or "20FR12B/A" or "20FR12B" or "DRSLTR" or "DRS/A" or "CB/A" or "CB" => "Prospectuses",
            "3" or "4" or "4/A" or "SC 13G/A" or "13F-HR" or "144" or "SC 13G" or "SC 13D" or "SC 13D/A" => "Ownership",
            "6-K" or "6-K/A" or "8-K" or "8-K/A" => "News",
            "DEF 14A" or "DEFA14A" or "PRE 14A" => "Proxies",
            _ => "Other"
        };
    }
}
