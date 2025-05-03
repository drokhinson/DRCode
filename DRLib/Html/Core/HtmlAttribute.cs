using DRLib.Html.Rendering;

namespace DRLib.Html.Core;

/// <summary>
/// Html attribute used to modify properties of an HtmlItem. Key value pair in the form `AttrName="Text"`
/// </summary>
public record HtmlAttribute(string Name, string Text);

public record HtmlStyle(string Property, string Value) : HtmlAttribute("style", $"{Property}: {Value}; ")
{
    protected HtmlStyle(string property, HtmlSize size) : this(property, size.ToHtmlString()) { }
}