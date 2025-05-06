using DRLib.Html.Core;

namespace DRLib.Html.Attributes;

public sealed record Link(string Text) : HtmlAttribute("href", Text);

public sealed record Source(string Text) : HtmlAttribute("src", Text);

public sealed record Alt(string Text) : HtmlAttribute("alt", Text);

public sealed record Id(string Text) : HtmlAttribute("id", Text);

public sealed record ColumnSpan(int Span) : HtmlAttribute("colspan", $"{Span}");