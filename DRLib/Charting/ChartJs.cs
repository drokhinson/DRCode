using DRLib.Html.Core;
using DRLib.Html.Scripts;

namespace DRLib.Charting;

public record ChartJsElement(string Id, Canvas Chart) : HtmlItem("canvas", new Html.Attributes.Id(Id))
{
    protected override void PreRenderActions()
    {
        Add(new ChartJsSrc());
        Add(new ChartJsDataScript(Id, Chart));
        base.PreRenderActions();
    }
}

public record ChartJsSrc : HtmlJScript
{
    public ChartJsSrc() => InitAttribute = new HtmlAttribute("src", @"https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.9.4/Chart.js");

    protected override string GetScriptText() => string.Empty;
}

public record ChartJsDataScript(string ChartName, Canvas Chart) : HtmlJScript(JScriptLoc.DoNotMove)
{
    protected override string GetScriptText()
    {
        var type = "line"; //todo this
        return
            $$"""
            const {ChartName} = new Chart("{{ChartName}}", {
                type: "{{type}}",
                data: {
                    {{GetData()}}
                },
                options: {
                    {{GetOptions()}}
                }
            """;
    }

    private string GetData()
    {
        return "";
    }

    private string GetOptions()
    {
        return "";
    }
}