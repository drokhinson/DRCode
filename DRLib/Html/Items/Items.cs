using System.Drawing;
using DRLib.Html.Core;

namespace DRLib.Html.Items;

/// <summary> Html block that can only appear once per html document </summary>
public interface IHtmlUnique;

public abstract record HtmlValueItem(string Tag, string Value) : HtmlItem(Tag)
{
    public sealed override string TextValue { get; set; } = Value;
}

public sealed record Doc : HtmlItem, IHtmlUnique
{
    public readonly Head Head;
    public readonly Body Body;
    public Doc() : base("html")
    {
        Head = Add(new Head());
        Body = Add(new Body());
    }
}

public sealed record Head() : HtmlItem("head"), IHtmlUnique;

public sealed record Title(string Text) : HtmlValueItem("title", Text), IHtmlUnique;

public sealed record Body() : HtmlItem("body"), IHtmlUnique;

public sealed record Main() : HtmlItem("main"), IHtmlUnique;

public sealed record Div() : HtmlItem("div");

public record Header(string Text, int H) : HtmlValueItem($"h{H}", Text);

public sealed record Paragraph(string Text = "") : HtmlValueItem("P", Text);

public sealed record Caption(string Text) : HtmlValueItem("caption", Text);

public sealed record Text(string Value) : HtmlValueItem("span", Value)
{
    public Text(string text, Color color) : this(text) => InitAttribute = new Styles.ForeColor(color);
}

public sealed record Link(string Text) : HtmlValueItem("a", Text)
{
    public Link(string text, string url) : this(text) => InitAttribute = new Attributes.Link(url);
}

public sealed record Image() : HtmlItem("img")
{
    public Image(string src) : this() => AddAttribute(new Attributes.Source(src));
    public Image(string text, string alt) : this(text) => InitAttribute = new Attributes.Alt(alt);
}

public sealed record Break() : HtmlItem("br")
{
    public override string TextValue => null;
}

public sealed record DividerLine() : HtmlItem("hr")
{
    public override string TextValue => null;
}

public record List() : HtmlItem("ul");

public record ListItem() : HtmlItem("li");

public record Meta() : HtmlItem("meta");