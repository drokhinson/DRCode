using DRLib.Html.Core;

namespace DRLib.Html.Css;

[Flags]
public enum ClassSelector
{
    None = 0,
    ClassName = 1 << 1,
    ElementTag = 1 << 2,
    ElementId = 1 << 3,
}

public record HtmlClass : HtmlAttribute {
    public string ClassName { get; init; }
    public List<HtmlStyle> Styles { get; init; }
    public List<HtmlStylesByTag> DescendantStyles { private get; init; } = [];
    
    public ClassSelector Selector { get; init; } = ClassSelector.ClassName | ClassSelector.ElementTag;

    public HtmlClass(string className, params HtmlStyle[] styles) : base("class", className)
    {
        Styles = styles.ToList();
        ClassName = className;
    }

    public List<CssRuleset> GetRuleSets(HtmlItem item)
    {
        var parentSelector = "";

        if (Selector == ClassSelector.None)
            parentSelector = "*";
        if (Selector.HasFlag(ClassSelector.ElementTag))
            parentSelector += CssSelector.FromElementTag(item.Tag);
        if (Selector.HasFlag(ClassSelector.ElementId))
            parentSelector += CssSelector.FromElementId(item.Id);
        if (Selector.HasFlag(ClassSelector.ClassName))
            parentSelector += CssSelector.FromClassName(ClassName);

        var parentRule = new CssRuleset(parentSelector, Styles.ToArray());

        var rules = new List<CssRuleset> { parentRule };
        rules.AddRange(GetDescendantStyles(parentSelector));

        return rules;
    }

    // Group styles across shared descendants.
    private List<CssRuleset> GetDescendantStyles(string parentSelector)
    {
        var flattened = DescendantStyles.SelectMany(tag => tag.Styles
            .Select(r => new { Tag = tag.ElementTag, Style = new HtmlStyle(r.Property, r.Value) })
        ).ToList();

        var styleGroups = flattened
            .GroupBy(r => r.Style)
            .Select(grp => new {
                Style = grp.Key,
                Selector = CssSelector.FromDescendantChain([parentSelector], [.. grp.Select(r => r.Tag)])
            })
            .GroupBy(r => r.Selector)
            .Select(grp => new CssRuleset(grp.Key, grp.Select(r => r.Style).ToArray()))
            .ToList();

        return styleGroups;
    }

    // Create unique style section for each tag, no grouping across styles.
    private List<CssRuleset> GetDescendantStyles_FLAT(string parentSelector)
    {
        return DescendantStyles
            .GroupBy(r => r.ElementTag)
            .Select(grp => new CssRuleset(
                CssSelector.FromDescendantChain([parentSelector, grp.Key]),
                grp.SelectMany(r => r.Styles).ToArray())
            ).ToList();
    }

    public void AddDescendantStyles(string childTag, params HtmlStyle[] childStyles)
    {
        DescendantStyles.Add(new HtmlStylesByTag(childTag, childStyles));
    }

    public void ApplyDescendantStyles(HtmlItem i)
    {
        foreach (var child in i.GetAllItems()) {
            foreach (var descClass in DescendantStyles) {
                if (child.Tag == descClass.ElementTag)
                    child.InsertAttributes(0, descClass.Styles);
            }

            ApplyDescendantStyles(child);
        }
    }
}

public record HtmlStylesByTag(string ElementTag, params HtmlStyle[] Styles);