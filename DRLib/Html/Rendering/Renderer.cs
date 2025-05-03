using System.Drawing;
using System.Text.RegularExpressions;
using DRLib.Html.Core;
using DRLib.Html.Css;
using DRLib.Html.Items;
using DRLib.Html.Scripts;

namespace DRLib.Html.Rendering;

public static class Renderer
{
    public static string ToHtmlString(this HtmlItem i) => RemoveCDataTag(i.ToXElement().ToString());

    public static string RenderHtml(this HtmlItem i)
    {
        // there's nowhere to put a style section with our CSS rules so render without CSS 
        if (i is not Doc && i is not Head)
            return i.RenderWithoutCss();

        // get all rules and make a style section
        var rules = GetCssRules(i);
        var styleSection = new HtmlStyleSection(rules);
        
        // add style section to document's head
        var head = i is Doc doc ? doc.Head : (Head)i;
        head.Add(styleSection);

        // add distinct scripts to document's head
        var scripts = i.GetScriptsToMove();
        var distinct = scripts
            .GroupBy(r => r.TextValue)
            .Select(r => r.First());
        head.AddRange(distinct);

        return RenderFromXElement(i);
    }

    private static List<CssRuleset> GetCssRules(HtmlItem i)
    {
        if (i is HtmlStyleSection)
            return new List<CssRuleset>();

        // Get all class definitions from item and children
        var rules = i.GetAttributesOfType<HtmlClass>().SelectMany(x => x.GetRuleSets(i)).ToList(); // add rules from i

        // Get all rules from HtmlStyleSection
        var styleSection = i.GetAllItems().OfType<HtmlStyleSection>().ToList();
        rules.AddRange(styleSection.SelectMany(r => r.CssRules));

        // add rules from children of i
        rules.AddRange(i.GetAllItems().SelectMany(GetCssRules)); 

        // Remove all HtmlStyleSection from body of doc, will be recreated in head
        i.RemoveAll(r => r is HtmlStyleSection);

        return rules;
    }

    private static List<HtmlJScript> GetScriptsToMove(this HtmlItem i, List<HtmlJScript> scripts = null)
    {
        scripts ??= new();

        var allItems = i.GetAllItems();

        var scriptsToAdd = allItems
            .OfType<HtmlJScript>()
            .Where(r => r.Loc is JScriptLoc.MoveToHead)
            .ToArray();

        scripts.AddRange(scriptsToAdd);

        i.RemoveAll(r => r is HtmlJScript { Loc: JScriptLoc.MoveToHead });

        foreach (var child in allItems.Except(scriptsToAdd))
            GetScriptsToMove(child, scripts);

        return scripts;
    }

    private static string RenderFromXElement(HtmlItem i)
    {
        var rendered = i.ToXElement().ToString();
        var html = RemoveCDataTag(rendered);
        return html;
    }

    public static string RemoveCDataTag(string html)
    {
        string pattern = @"<!\[CDATA\[(.*?)\]\]>";
        return Regex.Replace(html, pattern, match => match.Groups[1].Value, RegexOptions.Singleline);
    }


    public static string RenderWithoutCss(this HtmlItem i)
    {
        ApplyStylesFromHtmlClass(i); // find all HtmlStyleClass attributes and apply their styles directly to the element they are attached to
        return RenderFromXElement(i);

        static void ApplyStylesFromHtmlClass(HtmlItem i)
        {
            var classList = i.GetAttributesOfType<HtmlClass>().ToList();
            foreach (var styleClass in classList) {
                i.RemoveAttribute(styleClass);

                i.InsertAttributes(0, styleClass.Styles); // apply the class's styles
                styleClass.ApplyDescendantStyles(i); // apply styles to descendants
            }

            // recursively do the same for styles attached to children elements
            foreach (var child in i.GetAllItems())
                ApplyStylesFromHtmlClass(child);
        }
    }
    
    public static string ToHtmlString(this HtmlSize size)
    {
        return size.Match(
            pixelWidth => $"{pixelWidth}px",
            percentWidth => $"{percentWidth:P2}",
            str => string.IsNullOrEmpty(str) ? "auto" : str
        );
    }

    public static string ToHtmlString(this Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}