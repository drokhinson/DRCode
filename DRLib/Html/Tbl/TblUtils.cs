using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using DRLib.Html.Core;
using DRLib.Html.Utils;

namespace DRLib.Html.Tbl;

public static class HtmlTableUtils
{
    public static DataRow GetRow(bool hasRowHeader, params object[] cellValues)
    {
        var row = new DataRow();
        if (hasRowHeader)
            row.Add(new HeaderCell(cellValues[0].ToString()));
        var skip = hasRowHeader ? 1 : 0;
        row.AddRange(cellValues.Skip(skip).Select(r => new Cell(r)));
        return row;
    }

    public static HeaderRow GetHeaderRow(params string[] cellValues)
    {
        var row = new HeaderRow();
        row.AddRange(cellValues.Select(r => new HeaderCell(r)));
        return row;
    }

    public static Table ConvertToHtmlTable<T>(IEnumerable<T> obj, bool split_Char = false)
    {
        var table = new Table();
        var dataList = obj.ToList();
        var type = typeof(T);
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        props.RemoveAll(r => !r.CanRead || !r.GetGetMethod(true).IsPublic); // remove methods with private get

        var headerRow = table.Add(new HeaderRow());
        table.HeaderNames = props.Select(r => r.Name).ToArray();
        foreach (var prop in props) {
            var displayAttr = prop.GetCustomAttributes().OfType<DisplayNameAttribute>().FirstOrDefault();

            var propName = prop.Name;
            if (split_Char)
                propName = propName.Replace('_', ' ');

            var name = displayAttr?.DisplayName ?? propName;
            headerRow.Add(new HeaderCell(name));
        }

        foreach (var item in dataList) {
            var row = table.Add(new DataRow());
            foreach (var prop in props) {
                var value = prop.GetValue(item) ?? GetDefault(prop.PropertyType);

                var formatAttribute = prop.GetCustomAttributes().OfType<HtmlColumnFormat>().SingleOrDefault();
                if (formatAttribute != null)
                    value = string.Format(formatAttribute.Format, value);

                row.Add(new Cell(value));
            }
        }

        return table;

        static object GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
    }

    public static Items.Caption AddTitle(this Table t, string title, HtmlFontBuilder font = null)
    {
        var caption = new Items.Caption(title);
        if (font != null)
            caption.AddAttributes(font.GetStyles());

        t.Insert(t.GetAllItems().OfType<Items.Caption>().Count(), caption);
        return caption;
    }

    public static Items.Caption AddBottomCaption(this Table t, string text, bool rightSide = true, HtmlFontBuilder font = null)
    {
        var bottom = t.AddTitle(text, font);
        bottom.AddAttributes(new HtmlAttribute[] {
            new Styles.CaptionSide("bottom"),
            new Styles.TextAlign(rightSide ? "right" : "left")
        });
        return bottom;
    }

    public static Cell AddTriangle(this Cell c, bool up)
    {
        var val = up ? "▲" : "▼";
        var color = up ? Color.Green : Color.Red;
        var triangle = new Items.Text(val) {
            InitAttributes = [new HtmlStyle("position", "absolute"), new HtmlStyle("right", "5px")]
        }.Color(color);

        c.AddAttribute(new HtmlStyle("position", "relative"));
        c.Add(triangle);
        return c;
    }
}

/// <summary> C# class attribute used to specify .ToString() display format when using HtmlTableUtils.ConvertToHtmlTable() </summary>
public class HtmlColumnFormat(string format = null) : Attribute
{
    public string Format { get; } = format;
}
