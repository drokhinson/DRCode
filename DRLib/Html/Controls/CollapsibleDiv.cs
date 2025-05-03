using System.Drawing;
using DRLib.Html.Core;
using DRLib.Html.Css;
using DRLib.Html.Rendering;
using DRLib.Html.Scripts;
using DRLib.Html.Utils;

namespace DRLib.Html.Controls;

public sealed record CollapsibleDiv : HtmlItem
{
    private readonly Items.Div InnerDiv = new ();
    private readonly HtmlCollapseToggleButton Button;
    public readonly List<HtmlAttribute> ButtonAttributes = new();

    public CollapsibleDiv(string sectionLabel, Color? color = null, bool collapsed = true) : this(new Html.Items.Text(sectionLabel), color, collapsed) { }

    public CollapsibleDiv(Items.Text sectionLabel, Color? color = null, bool collapsed = true) : base("collapse")
    {
        AddAttribute(new CollapseSection(color ?? Color.White));

        Button = new HtmlCollapseToggleButton(sectionLabel);
        Button.Add(new ToggleCollapseScript());

        InnerDiv.AddAttribute(new CollapsibleContent());
        if (!collapsed)
            InnerDiv.AddAttribute(new Styles.Height("auto"));

        base.Add(Button);
        base.Add(InnerDiv);
    }

    public override T Add<T>(T item)
    {
        InnerDiv.Add(item);
        return item;
    }

    public override void AddRange<T>(IEnumerable<T> items)
    {
        InnerDiv.AddRange(items);
    }

    protected override void PreRenderActions()
    {
        base.PreRenderActions();
        Button.AddAttributes(ButtonAttributes);
    }

    public record ToggleCollapseScript : HtmlJScript
    {
        protected override string GetScriptText()
        {
            return """
                   function toggle(elem){
                       sec=elem.parentElement;
                       if(sec.style.width!='100%') sec.style.width='100%'; else sec.style.width='auto';
                       coll=sec.getElementsByClassName("collapsible-content")[0];
                       if(coll.style.height!='auto') coll.style.height='auto'; else coll.style.height='0px';
                   }
                   """;
        }
    }

    public record CollapseSection(Color Color) : HtmlClass($"CollapseSection_{Color.ToHtmlString()}",
        new HtmlStyle("display", "inline-block"),
        new HtmlStyle("transition", ".25s"),
        new HtmlStyle("width", "auto"),
        new Styles.BackColor(Color));

    public record CollapsibleContent() : HtmlClass("collapsible-content", new Html.Styles.Height(0), new HtmlStyle("overflow", "hidden"));

    public sealed record HtmlCollapseToggleButton : HtmlItem
    {
        public HtmlCollapseToggleButton(Html.Items.Text Text) : base("button")
        {
            AddAttributes([
                new HtmlAttribute("type", "button"),
                new HtmlAttribute("class", "collapsible"),
                new HtmlAttribute("onclick", "toggle(this);")
            ]);
            Add(Text);
        }
    }
}