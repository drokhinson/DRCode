using System.Drawing;
using DRLib.Html.Core;
using DRLib.Html.Rendering;
using DRLib.Html.Scripts;

namespace DRLib.Charting;

public sealed record ChartJsElement : HtmlItem
{
    private readonly Chart Chart;
    
    public ChartJsElement(string id, Chart chart) : base("canvas")
    {
        Id = id;
        Chart = chart;
        Add(new ChartJsSrc());
    }
    
    protected override void PreRenderActions()
    {
        Add(new ChartJsDataScript(Id, Chart));
        base.PreRenderActions();
    }
}

public record ChartJsSrc : HtmlJScript
{
    public ChartJsSrc()
    {
        InitAttribute = new HtmlAttribute("src", @"https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.9.4/Chart.js");
    } 

    protected override string GetScriptText() => string.Empty;
}

public record ChartJsDataScript(string ChartName, Chart Chart) : HtmlJScript(JScriptLoc.DoNotMove)
{
    public bool IsMixedChart;
    
    protected override string GetScriptText()
    {
        return
            $$"""
            
            const {{ChartName}} = new Chart("{{ChartName}}", 
            {
                type: "{{GetChartType(Chart.Objects.First())}}",
                data: {
                    {{GetDataSets()}}
                },
                options: {
                    {{GetOptions()}}
                }
            })
            
            """;
    }

    private static string GetChartType(ChartObj obj)
    {
        return obj switch {
            BarPlot => "bar",
            HorizontalBarPlot => "horizontalBar",
            //PiePlot => "pie",
            //DonutChart => "doughnut",
            LinePlot => "line",
            ScatterPlot => "scatter",
        };
    }
    
    private string GetDataSets()
    {
        var sets = Chart.Objects.OfType<PointPlot>().Select(GetDataSet);
        return $"datasets: [\n{string.Join(",\n", sets)}\n]";
    }

    private static string GetDataSet(PointPlot pp)
    {
        var pointString = string.Join(", \n", pp.Points.Select(GetPointString));
        return
            $$"""
              {
              type: "{{GetChartType(pp)}}",
              label: "{{pp.Name}}",
              data: [ {{pointString}} ],
              borderColor: '{{pp.Color.ToHtmlString()}}',
              backgroundColor: '{{pp.Color.ToHtmlString()}}',
              }
            """;
    }
    
    private static string GetPointString(Point p)
    {
        return $$"""{x:{{p.X}},y:{{p.Y}}}""";
    }

    private string GetOptions()
    {
        return "";
    }
}