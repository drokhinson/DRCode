using DRLib.Html.Core;
using DRLib.Html.Items;
using DRTest.Html.UserUI.Attributes;

namespace DRTest.Html.UserUI.Elements
{
    /// <summary> UI object that has user interactions </summary>
    public abstract record Control : HtmlItem
    {
        /// <summary>
        /// An <input> element in HTML is a versatile and fundamental component used to create interactive form controls that allow users to enter data
        /// </summary>

        /// <param name="type"> Specifies the type of data the input should accept </param>
        /// <param name="name"> Name attribute of an <input> element is used to identify the form data associated with that input when the form is submitted </param>
        protected Control(string id, string tag, string type = null, string name = null, string value = null) : base(tag)
        {
            Id = id;
            if (type != null)
                AddAttribute(new InputType(type));
            if (value != null)
                Value = value;
            name ??= $"{type ?? tag}_{id}";
            AddAttribute(new Name(name));
        }

        /// <summary> Value attribute specifies the default value of the element. </summary>
        public string Value { set => AddAttribute(new InputValue(value)); }

        public Label AddLabel(string value) => Add(new Label(value, Id));
    }

    public abstract record InputControl : Control
    {
        protected InputControl(string id, string type, string name = null, string value = null) : base(id, "input", type, name, value) { }
    }

    public record Label : HtmlValueItem
    {
        public Label(string text, string forId) : base("label", text)
        {
            AddAttribute(new For(forId));
        }

        public Label(string text, HtmlItem forItem) : this("label", forItem.Id) { }

        /// <summary> The for attribute is used in HTML with <label> elements to explicitly associate a label with a form element. </summary>
        public record For(string Id) : HtmlAttribute("for", Id);
    }
}

namespace DRTest.Html.UserUI.Elements.Text
{
    /// <summary> Allows users to enter any text. It's the most basic input type. </summary>
    public record TextBox(string Id) : InputControl(Id, "text");

    /// <summary> Similar to text, but the input is obscured (typically shown as dots or asterisks) for security. </summary>
    public record Password(string Id) : InputControl(Id, "password");

    /// <summary> Validates that the input is a properly formatted email address. </summary>
    public record Email(string Id) : InputControl(Id, "email");

    /// <summary> Validates that the input is a properly formatted URL. </summary>
    public record Url(string Id) : InputControl(Id, "url");

    /// <summary> Provides a search field. Some browsers may style this differently, often with a clear button. </summary>
    public record Search(string Id) : InputControl(Id, "search");

    /// <summary> Intended for phone numbers. It does not validate the format but may provide a numeric keypad on mobile devices. </summary>
    public record Telephone(string Id) : InputControl(Id, "tel");
}

namespace DRTest.Html.UserUI.Elements.Buttons
{
    /// <summary> A generic button that can be programmed to perform any action. </summary>
    public record Button : Control
    {
        public Button(string id, string text) : base(id, "button")
        {
            TextValue = text;
        }
    }

    /// <summary> Submits the form data to the server. </summary>
    public record Submit(string Id) : InputControl(Id, "button", "submit");

    /// <summary> Resets all form fields to their default values. </summary>
    public record Reset(string Id) : InputControl(Id, "button", "reset");
}

namespace DRTest.Html.UserUI.Elements.Numeric
{
    /// <summary> Allows users to enter a number. You can specify min, max, and step attributes. </summary>
    public record NumberBox(string Id) : InputControl(Id, "number");

    /// <summary> Provides a slider control for selecting a number within a range. </summary>
    public record Range(string Id) : InputControl(Id, "range");
}

namespace DRTest.Html.UserUI.Elements.Date
{
    /// <summary> Allows users to select a date from a calendar. </summary>
    public record DatePicker(string Id) : InputControl(Id, "date");

    /// <summary> Allows users to select a time. </summary>
    public record TimePicker(string Id) : InputControl(Id, "time");

    /// <summary> Allows users to select both date and time, without time zone information. </summary>
    public record DateTimePicker(string Id) : InputControl(Id, "datetime-local");

    /// <summary>  Allows users to select a month and year. </summary>
    public record MonthPicker(string Id) : InputControl(Id, "month");

    /// <summary> Allows users to select a week and year. </summary>
    public record WeekPicker(string Id) : InputControl(Id, "week");
}

namespace DRTest.Html.UserUI.Elements.Specialized
{
    /// <summary> Provides a color picker for selecting a color value. </summary>
    public record Color(string Id) : InputControl("color", Id);

    /// <summary> Allows users to select one or more options by checking boxes. </summary>
    public record CheckBox(string Id, string Name, string Text) : InputControl("checkbox", Id, Name, Text);

    /// <summary> Allows users to select one option from a group of options. </summary>
    public record Radio(string Id, string Name, string Text) : InputControl("radio", Id, Name, Text);

    /// <summary> Allows users to upload files from their device. </summary>
    public record File(string Id, string Name) : InputControl("file", Id, Name);

    /// <summary> Stores data that is not visible to users but can be submitted with the form. </summary>
    public record Hidden(string Id, string Name) : InputControl("hidden", Id, Name);
}