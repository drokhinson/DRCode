using System.Drawing;
using DRLib.Html.Core;
using DRLib.Html.Items;

namespace DRLib.Html.Tbl;

public record Table() : HtmlItem("table")
{
    public THead THead => GetOrAdd<THead>();
    public TBody TBody => GetOrAdd<TBody>();

    public HtmlItem this[int row, int col] => this[row][col];
    public HtmlItem this[int row, string colName] => this[row][colName];
    public Row this[int row] => TBody.GetAllItems().OfType<Row>().ElementAt(row);

    public Row[] Rows => TBody.GetAllItems().OfType<Row>().ToArray();

    /// <summary> When set, allows cell look up by column header name </summary>
    public string[] HeaderNames = null;


    public override T Add<T>(T item)
    {
        if (item is Row r)
            r.HeaderNames = HeaderNames;

        if (item is HeaderRow)
            return THead.Add(item);

        if (item is DataRow)
            return TBody.Add(item);

        return base.Add(item);
    }

    public override void AddRange<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
            Add(item);
    }

    private T GetOrAdd<T>() where T : HtmlItem, new()
    {
        var val = GetItemsOfType<T>().FirstOrDefault(); // Can have multiple tBody sections (not recommended)
        if (val == null) {
            val = new T();
            Add(val);
        }

        return val;
    }
}

/// <summary> Section containing all header rows </summary>
public record THead() : HtmlItem("thead");

/// <summary> Section containing all data rows </summary>
public record TBody() : HtmlItem("tbody");

public abstract record Row() : HtmlItem("tr")
{
    /// <summary> When set, allows cell look up by column header name </summary>
    public string[] HeaderNames;

    public Cell this[int col] => col == -1 ? null : GetAllItems().OfType<Cell>().ElementAt(col);
    public Cell this[string colName] => this[Array.IndexOf(HeaderNames, colName)];
}

public record HeaderRow : Row;

public record DataRow : Row;

public sealed record HeaderCell(string Text) : HtmlValueItem("th", Text);

public sealed record Cell(string Text) : HtmlValueItem("td", Text)
{
    public Cell(object val, Color? color = null) : this(val is HtmlItem ? string.Empty : val?.ToString())
    {
        if (val is HtmlItem html)
            Add(html);
        if (color.HasValue)
            InitAttribute = new Html.Styles.BackColor(color.Value);
    }
}