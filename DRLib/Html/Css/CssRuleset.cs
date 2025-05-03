using DRLib.Html.Core;

namespace DRLib.Html.Css;

public record CssRuleset 
{
    public string Selector { get; init; }
    public List<HtmlStyle> StyleRules { private get; init; } = new();

    public CssRuleset(string selector, params HtmlStyle[] styleRules)
    {
        Selector = selector;
        StyleRules = styleRules.ToList();
    }

    public string GetCssText()
    {
        var ruleText = string.Join("\n", StyleRules.Select(x => $"\t\t{x.Text}"));
        return $"\t{Selector} {{\n{ruleText}\n\t}}";
    }

    /// <summary> selects elements with the specified value for their class attribute </summary>
    public static CssRuleset FromClassName(string className, params HtmlStyle[] styles) => new(CssSelector.FromClassName(className), styles);

    /// <summary> creates style rules applied to elements with matching ElementTags </summary>
    public static CssRuleset FromElementTag(string elementType, params HtmlStyle[] styles) => new(elementType, styles);

    /// <summary> creates style rules applied to elements with matching ElementTags </summary>
    public static CssRuleset FromElementTags(string[] elementTypes, params HtmlStyle[] styles) => new(CssSelector.FromElementTag(elementTypes), styles);

    /// <summary> creates style rules applied to elements with specified value for their ID attribute </summary>
    public static CssRuleset FromElementId(string id, params HtmlStyle[] styles) => new(CssSelector.FromElementId(id), styles);
}

public sealed record HtmlStyleSection(List<CssRuleset> CssRules) : HtmlItem("style")
{
    public HtmlStyleSection(params CssRuleset[] rules) : this(rules.ToList()) { }
    
    public void AddRule(CssRuleset ruleSet) => CssRules.Add(ruleSet);

    public override string TextValue => GetCssText();

    public string GetCssText()
    {
        var rules = CssRules.Select(x => x.GetCssText()).ToList();
        rules = rules.Distinct().ToList();
        return $"\n{string.Join("\n", rules)}\n";
    }
}

public static class CssSelector
{
    public static string FromClassName(string className) => $".{className}";
    
    public static string FromElementId(string id) => $"#{id}";

    public static string FromElementTag(params string[] tags) => string.Join(", ", tags);

    public static string FromDescendantChain(string[] descendsFrom) => string.Join(" ", descendsFrom);
    
    public static string FromDescendantChain(string[] descendsFrom, string[] targetTags) => string.Join(" ", [.. descendsFrom, FromElementTag(targetTags)]);

    public static string FromAscSiblingTags(params string[] orderedSiblings) => string.Join(" + ", orderedSiblings);

    public static string FromChildChain(string[] childOfChain) => string.Join(" > ", childOfChain);
}