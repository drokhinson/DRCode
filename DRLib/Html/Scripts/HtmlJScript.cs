using DRLib.Html.Attributes;
using DRLib.Html.Core;

namespace DRLib.Html.Scripts;

public enum JScriptLoc
{
    MoveToHead,         // Move script to head/top of document
    DoNotMove,          // Leaves script in line next to parent element
    DoNotMove_Defer,    // Leaves script in line but appends 'defer' attribute 
}

public abstract record HtmlJScript : HtmlItem
{
    public JScriptLoc Loc { get; private init; }

    public HtmlJScript(JScriptLoc loc = JScriptLoc.MoveToHead) : base("script")
    {
        Loc = loc;
        if (loc == JScriptLoc.DoNotMove_Defer)
            AddAttribute(new EmptyHtmlAttribute("defer"));

        if (loc == JScriptLoc.MoveToHead)
            Id = GetType().Name;
    }

    public override string TextValue => GetScriptText();
    public override bool ConvertHtmlCharacters => false;

    protected abstract string GetScriptText();
}