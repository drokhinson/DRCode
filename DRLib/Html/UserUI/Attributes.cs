using DRLib.Html.Core;

namespace DRLib.Html.UserUI.Attributes;

/// <summary> Identifies the input field and is used to reference the input data when the form is submitted. </summary>
public record InputType(string Value) : HtmlAttribute("type", Value);

/// <summary> Sets the default value for the input field. </summary>
public record InputValue(string Value) : HtmlAttribute("type", Value);

/// <summary> Provides a hint to the user about what to enter in the input field. </summary>
public record Placeholder(string Hint) : HtmlAttribute("placeholder", Hint);

/// <summary> Makes the input field non-editable. </summary>
public record Required() : EmptyHtmlAttribute("required");

/// <summary> Makes the input field non-editable. </summary>
public record Readonly() : EmptyHtmlAttribute("readonly");

/// <summary> Disables the input field, preventing user interaction. </summary>
public record Disabled() : EmptyHtmlAttribute("readonly");

/// <summary> Disables the input field, preventing user interaction. </summary>
public record Pattern(string Validate) : HtmlAttribute("pattern", Validate);

/// <summary> Sets the minimum value for the quantity input. </summary>
public record Min(string Validate) : HtmlAttribute("min", Validate);

/// <summary> Sets the maximum value for the quantity input. </summary>
public record Max(string Validate) : HtmlAttribute("min", Validate);

/// <summary> Specifies that the quantity input should increment. </summary>
public record Step(string Validate) : HtmlAttribute("min", Validate);

/// <summary> Specifies the type of data the input should accept (e.g., text, password, email, number, etc.). </summary>
public record Name(string Value) : HtmlAttribute("name", Value);