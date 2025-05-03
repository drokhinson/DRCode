using DRLib.Html.Core;
using DRLib.Html.Scripts;

namespace DRLib.Html.Controls;

/// <summary> Insert formatted html text as an html item </summary>
public sealed record HtmlIFrame : HtmlItem
{
    public HtmlIFrame(string htmlText, bool isFilePath, bool adjustToDocSize = true) : base("iframe")
    {
        InitAttribute = isFilePath ?
            new Attributes.Source(htmlText) :
            new HtmlAttribute("srcdoc", htmlText);
        AddAttribute(new Css.Fill());
        AddAttribute(new HtmlAttribute("boarder", "none"));
        AddAttribute(new HtmlAttribute("overflow", "hidden"));
        if (adjustToDocSize) {
            Add(new ResizeIframe());
            AddAttribute(new HtmlAttribute("onload", "resizeIframe(this)"));
        }
    }

    public record ResizeIframe() : HtmlJScript(JScriptLoc.DoNotMove_Defer)
    {
        protected override string GetScriptText()
        {
            return """
                   function resizeIframe(iframe) {
                       iframe.style.height = iframe.contentWindow.document.body.scrollHeight + 'px';
                   }
                   """;
        }
    }
}
