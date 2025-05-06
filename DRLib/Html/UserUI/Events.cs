using DRLib.Html.Core;
using DRLib.Html.Scripts;

namespace DRLib.Html.UserUI.Events
{
    public delegate string HtmlEventHandler(object sender, HtmlEventArgs e);

    public class HtmlEventArgs : EventArgs
    {
        public string CallerId { get; set; }
        public string EventType { get; set; }
        public string TargetValue { get; set; }

        public override string ToString() => $"{CallerId}_{EventType}";
    }

    public abstract record TriggerCSharpEvent(string Event) : HtmlAttribute(Event, "callCSharp(event)")
    {
        public HtmlEventHandler EventAction { get; set; }
    }

    public record CallCSharpJScript(string Host = CallCSharpJScript.DefaultHost) : HtmlJScript
    {
        public const string DefaultHost = "http://localhost:5000/";

        protected override string GetScriptText()
        {
            return
                $$"""
                
                function callCSharp(event) {
                    console.log(event);
                    const callerId = event.target.id;
                    console.log(`Caller ID: ${callerId}`);

                    fetch("{{Host}}", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ 
                		    {{nameof(HtmlEventArgs.CallerId)}}: callerId,
                		    {{nameof(HtmlEventArgs.EventType)}}: event.type,
                		    {{nameof(HtmlEventArgs.TargetValue)}}: event.target.value
                	    })
                    })
                    .then(response => response.text())
                    .then(data => console.log(data))
                    .catch(error => console.error('Error:', error));
                }
                
                """;
        }
    }

}

namespace DRLib.Html.UserUI.Events.Mouse
{
    /// <summary> Triggered when the element is clicked </summary>
    public record OnClick() : TriggerCSharpEvent("onclick");

    /// <summary> Triggered when the element is double-clicked </summary>
    public record OnDblClick() : TriggerCSharpEvent("ondblclick");

    /// <summary> Triggered when the mouse button is pressed down on the element </summary>
    public record OnMouseDwn() : TriggerCSharpEvent("onmousedown");

    /// <summary> Triggered when the mouse button is released over the element </summary>
    public record OnMouseUp() : TriggerCSharpEvent("onmouseup");

    /// <summary> Triggered when the mouse pointer enters the element's area </summary>
    public record OnMouseOver() : TriggerCSharpEvent("onmouseover");

    /// <summary> Triggered when the mouse pointer leaves the element's area </summary>
    public record OnMouseOut() : TriggerCSharpEvent("onmouseout");

    /// <summary> Triggered when the mouse pointer moves over the element </summary>
    public record OnMouseMove() : TriggerCSharpEvent("onmousemove");
}

namespace DRLib.Html.UserUI.Events.Keyboard
{
    /// <summary> Triggered when a key is pressed down </summary>
    public record OnKeyDwn() : TriggerCSharpEvent("onkeydown");

    /// <summary> Triggered when a key is released </summary>
    public record OnKeyUp() : TriggerCSharpEvent("onkeyup");
}

namespace DRLib.Html.UserUI.Events.Focus
{
    /// <summary> Triggered when the element gains focus </summary>
    public record OnFocus() : TriggerCSharpEvent("onfocus");

    /// <summary> Triggered when the element loses focus </summary>
    public record OnBlur() : TriggerCSharpEvent("onblur");
}

namespace DRLib.Html.UserUI.Events.Form
{
    /// <summary> Triggered when the value of an element changes and loses focus (typically used with input elements) </summary>
    public record OnChange() : TriggerCSharpEvent("onchange");

    /// <summary> Triggered when a form is submitted </summary>
    public record OnSubmit() : TriggerCSharpEvent("onsubmit");

    /// <summary> Triggered when the mouse button is pressed down on the element </summary>
    public record OnReset() : TriggerCSharpEvent("onreset");

    /// <summary> Triggered when a form is reset </summary>
    public record OnInput() : TriggerCSharpEvent("oninput");
}

namespace DRLib.Html.UserUI.Events.Window
{
    /// <summary> Triggered when the window or an element has finished loading. </summary>
    public record OnLoad() : TriggerCSharpEvent("onload");

    /// <summary> Triggered when the window or an element is about to be unloaded. </summary>
    public record OnUnload() : TriggerCSharpEvent("onunload");

    /// <summary> Triggered when the window is resized. </summary>
    public record OnResize() : TriggerCSharpEvent("onresize");

    /// <summary> Triggered when the window or an element is scrolled. </summary>
    public record OnScroll() : TriggerCSharpEvent("onscroll");
}

namespace DRLib.Html.UserUI.Events.MobileTouch
{
    /// <summary> Triggered when a touch point is placed on the touch surface. </summary>
    public record OnTouchStart() : TriggerCSharpEvent("ontouchstart");

    /// <summary> Triggered when a touch point is removed from the touch surface. </summary>
    public record OnTouchEnd() : TriggerCSharpEvent("ontouchend");

    /// <summary> Triggered when a touch point is moved along the touch surface. </summary>
    public record OnTouchMove() : TriggerCSharpEvent("ontouchmove");

    /// <summary> Triggered when a touch point is interrupted or canceled. </summary>
    public record OnTouchCancel() : TriggerCSharpEvent("ontouchcancel");
}