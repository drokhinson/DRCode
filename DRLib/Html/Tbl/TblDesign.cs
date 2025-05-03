using AIMLib.HtmlWriter.Tbl;
using System.Drawing;
using DRLib.Html.Core;
using DRLib.Html.Css;
using DRLib.Html.Rendering;
using DRLib.Html.Styles;
using DRLib.Html.Tbl;

// ReSharper disable once CheckNamespace
namespace AIMLib.HtmlWriter.Tbl.Design;

public record BorderStyle(int PxSize = 1, string Style = "solid", bool Collapsed = true, Color? C = null) :
    HtmlStyle($"{(Collapsed ? "border-collapse: collapse; " : "")}border",
        $"{PxSize}px {Style}{(C != null ? $" #{C.Value.ToHtmlString()}" : "")}");

public record HtmlTableDesign : HtmlClass
{
    public HtmlTableDesign(string className, params HtmlStyle[] styles) : base(className, styles) { }
    
    public HtmlStyle[] CellStyles { set => AddDescendantStyles("td", value); }
    public HtmlStyle[] HeaderCellStyles { set => AddDescendantStyles("th", value); }
    public HtmlStyle[] RowStyles { set => AddDescendantStyles("tr", value); }
}
public record BasicTableWBorder : HtmlTableDesign
{
    public BasicTableWBorder() : base("BasicTableWBorder", new BorderStyle(), new BackColor(Color.White))
    {
        CellStyles = new HtmlStyle[] {
            new BorderStyle(),
            new BackColor(Color.White),
            new Padding("4px")
        };
        HeaderCellStyles = new HtmlStyle[] {
            new BorderStyle(),
            new BackColor(Color.White),
            new Padding("4px")
        };
    }
}

public record DataGridViewDesign : HtmlTableDesign
{
    public DataGridViewDesign() : base("DataGridViewTableDesign",
        new BorderStyle(),
        new FontFamily("Consolas"))
    {
        CellStyles = new HtmlStyle[] {
            new BorderStyle(1, C: Color.FromArgb(221, 221, 221)),
            new Padding("8px")
        };
        HeaderCellStyles = new HtmlStyle[] {
            new BorderStyle(1, C: Color.FromArgb(221, 221, 221)),
            new Padding("8px"),
            new BackColor(Color.FromArgb(242, 242, 242)),
            new BoldFont()
        };
    }
}
