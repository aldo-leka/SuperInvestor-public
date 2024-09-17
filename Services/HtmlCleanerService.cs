using HtmlAgilityPack;
using Microsoft.AspNetCore.Components;

namespace SuperInvestor.Services;

public class HtmlCleanerService(NavigationManager navigationManager)
{
    public void CleanHtmlDocument(HtmlDocument htmlDocument, Uri secUri)
    {
        RemoveTags(htmlDocument, "meta");
        RemoveHeadContent(htmlDocument);
        RemoveHiddenDivs(htmlDocument);
        RemoveTags(htmlDocument, "script");
        RemoveTags(htmlDocument, "style");
        RemoveTags(htmlDocument, "link", node => node.GetAttributeValue("rel", "") == "stylesheet");
        RemoveTags(htmlDocument, "br");
        RemoveTags(htmlDocument, "hr");
        RemoveTags(htmlDocument, "template");
        RemoveNamespaces(htmlDocument.DocumentNode);
        RemoveComments(htmlDocument);
        FixHrefTags(htmlDocument);
        FixImageUrls(htmlDocument, secUri);
        RemoveHtmlHeadBodyTags(htmlDocument);
        htmlDocument.OptionFixNestedTags = true;
        htmlDocument.OptionAutoCloseOnEnd = true;
    }

    private static void RemoveNamespaces(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Element)
        {
            // Strip the namespace from the node name
            var nameParts = node.Name.Split(':');
            if (nameParts.Length > 1)
            {
                var parentNode = node.ParentNode;
                var innerHtml = node.InnerHtml;

                // Create a new HtmlDocument to parse the inner HTML
                var newDoc = new HtmlDocument();
                newDoc.LoadHtml(innerHtml);

                // Insert the child nodes of the new document into the parent node
                foreach (var child in newDoc.DocumentNode.ChildNodes)
                {
                    parentNode.InsertBefore(child, node);
                }

                // Remove the original node
                parentNode.RemoveChild(node);
            }
        }

        // Create a list to avoid modifying the collection during iteration
        var childNodes = node.ChildNodes.ToList();

        // Recursively remove namespaces from child nodes
        foreach (var childNode in childNodes)
        {
            RemoveNamespaces(childNode);
        }
    }

    private static void RemoveHeadContent(HtmlDocument htmlDocument)
    {
        var headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");
        headNode?.RemoveAllChildren();
    }

    private static void RemoveHiddenDivs(HtmlDocument htmlDocument)
    {
        var hiddenDivs = htmlDocument.DocumentNode.SelectNodes("//div[@style='display:none']");
        if (hiddenDivs != null)
        {
            foreach (var div in hiddenDivs)
            {
                div.Remove();
            }
        }
    }

    private static void RemoveTags(HtmlDocument htmlDocument, string tagName, Func<HtmlNode, bool> predicate = null)
    {
        var nodes = htmlDocument.DocumentNode.SelectNodes($"//{tagName}");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                if (predicate == null || predicate(node))
                {
                    node.Remove();
                }
            }
        }
    }

    private static void RemoveComments(HtmlDocument htmlDocument)
    {
        var comments = htmlDocument.DocumentNode.SelectNodes("//comment()");
        if (comments != null)
        {
            foreach (var comment in comments)
            {
                comment.Remove();
            }
        }
    }

    private void FixHrefTags(HtmlDocument htmlDocument)
    {
        var aTags = htmlDocument.DocumentNode.SelectNodes("//a");
        if (aTags != null)
        {
            foreach (var aTag in aTags)
            {
                aTag.Attributes.Remove("style");
                var hrefValue = aTag.GetAttributeValue("href", string.Empty);
                var newHrefValue = $"{navigationManager.Uri}{hrefValue}";
                aTag.SetAttributeValue("href", newHrefValue);
            }
        }
    }

    /// <summary>
    /// Example:
    /// Converts /aapl-20240629_g1.jpg to:
    /// https://www.sec.gov/Archives/edgar/data/320193/000032019324000081/aapl-20240629_g1.jpg
    /// </summary>
    /// <param name="htmlDocument"></param>
    /// <param name="secUri"></param>
    private static void FixImageUrls(HtmlDocument htmlDocument, Uri secUri)
    {
        var images = htmlDocument.DocumentNode.SelectNodes("//img[@src]");
        if (images != null)
        {
            foreach (var img in images)
            {
                var src = img.GetAttributeValue("src", string.Empty);
                if (!string.IsNullOrEmpty(src))
                {
                    var imgUrl = $"https://{secUri.Host}{string.Concat(secUri.Segments.SkipLast(1))}{src}";
                    img.SetAttributeValue("src", imgUrl);
                }
            }
        }
    }

    private static void RemoveHtmlHeadBodyTags(HtmlDocument htmlDocument)
    {
        var htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//html");
        if (htmlNode != null)
        {
            // Move all children of <body> to the root
            var bodyNode = htmlNode.SelectSingleNode("//body");
            if (bodyNode != null)
            {
                foreach (var child in bodyNode.ChildNodes.ToList())
                {
                    htmlDocument.DocumentNode.AppendChild(child);
                }
            }

            // Remove <html>, <head>, and <body> tags
            htmlNode.Remove();
        }
    }
}