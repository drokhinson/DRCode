using DRLib.Html.Core;

namespace DRLib.Html.Attributes;

// Some attributes don't take a value. Specifying the empty string is equivalent.
// https://www.w3.org/TR/2012/WD-html-markup-20120329/syntax.html#syntax-attributes
public sealed record EmptyHtmlAttribute(string Name) : HtmlAttribute(Name, string.Empty);

public sealed record Link(string Text) : HtmlAttribute("href", Text);

public sealed record Source(string Text) : HtmlAttribute("src", Text);

public sealed record Alt(string Text) : HtmlAttribute("alt", Text);

public sealed record Id(string Text) : HtmlAttribute("id", Text);

public sealed record ColumnSpan(int Span) : HtmlAttribute("colspan", $"{Span}");