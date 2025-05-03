using DRLib.Html.Core;

namespace DRLib.Html.Scripts;

/// <summary>
/// Load script from src url
/// </summary>
public record ScriptFromSrc : HtmlJScript
{
    public ScriptFromSrc(string src) => InitAttribute = new HtmlAttribute("src", src);

    protected override string GetScriptText() => string.Empty;
}