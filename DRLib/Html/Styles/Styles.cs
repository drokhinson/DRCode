using System.Drawing;
using DRLib.Html.Core;
using DRLib.Html.Rendering;

namespace DRLib.Html.Styles;

public sealed record CaptionSide(string Text) : HtmlStyle("caption-side", Text);

public sealed record Display(string Text) : HtmlStyle("display", Text);

public record Padding(HtmlSize Size) : HtmlStyle("padding", Size);

public record FontFamily(string Value) : HtmlStyle("font-family", Value);

public record TextAlign(string Value) : HtmlStyle("text-align", Value);

public record VerticalAlign(string Value) : HtmlStyle("vertical-align", Value);

public record BackColor(Color Color) : HtmlStyle("background-color", Color.ToHtmlString());

public record ForeColor(Color Color) : HtmlStyle("color", Color.ToHtmlString());

public record FontSize(HtmlSize Size) : HtmlStyle("font-size", Size);

public record FontWeight(string Text) : HtmlStyle("font-weight", Text);

public record BoldFont() : FontWeight("bold");

public record ItalicFont() : HtmlStyle("font-style", "italic");

public record Width(HtmlSize Size) : HtmlStyle("width", Size);

public record Height(HtmlSize Size) : HtmlStyle("height", Size);

public record GridColumnSpan(int StartCol, int SpanLength) : HtmlStyle("grid-column", $"{StartCol} / span {SpanLength}");

public record GridGap(HtmlSize Size) : HtmlStyle("grid-gap", Size);