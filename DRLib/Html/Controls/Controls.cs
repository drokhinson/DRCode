

using DRLib.Html.Attributes;
using DRLib.Html.Core;
using DRLib.Html.Items;
// ReSharper disable once CheckNamespace
using DRLib.Html;

namespace AIMLib.HtmlWriter.Controls;

public record InputForm() : HtmlItem("form");

public record Label(string Text) : HtmlValueItem("label", Text)
{
    public Label(string text, string forId) : this(text) => AddAttribute(new HtmlAttribute("for", forId));
    public Label(string text, InputControl c) : this(text) => AddAttribute(new HtmlAttribute("for", c.Id));
}

/// <param name="Id">Id attribute specifies a unique id for an HTML element. The value of the Id attribute must be unique within the HTML document.</param>
/// <param name="Name">Name is  used as a reference when the data is submitted</param>
public abstract record InputControl(string Type, string Id, string Name) :
    HtmlItem("input", new HtmlAttribute("type", Type), new Id(Id), new HtmlAttribute("name", Name))
{
    /// Value attribute specifies the initial value of the element.
    public string Value { set => AddAttribute(new HtmlAttribute("value", value)); }
    // Adds a label bound to this control
    public string Label { set => Add(new Label(value, Id)); }
}

public record SubmitButton(string Id) : InputControl("submit", Id, "submit");

public record ResetButton(string Id) : InputControl("reset", Id, "reset");

public record TextInput(string Id, string Name) : InputControl("text", Id, Name);

public record RadioButton(string Id, string Name) : InputControl("radio", Id, Name);

public record CheckBox(string Id, string Name) : InputControl("checkbox", Id, Name);

public record Progress(string Id, int Value, int MaxValue) : HtmlItem("progress",
    new HtmlAttribute("id", Id),
    new HtmlAttribute("value", $"{Value}"),
    new HtmlAttribute("max", $"{MaxValue}"));
