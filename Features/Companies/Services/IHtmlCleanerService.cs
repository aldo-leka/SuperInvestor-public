using HtmlAgilityPack;

namespace SuperInvestor.Features.Companies.Services;

public interface IHtmlCleanerService
{
    void CleanHtmlDocument(HtmlDocument htmlDocument, Uri secUri);
    void FixHrefTags(HtmlDocument htmlDocument);
}