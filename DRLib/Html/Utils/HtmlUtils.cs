using System.Drawing;
using DRLib.Html.Core;
using DRLib.Html.Items;

namespace DRLib.Html.Utils;

public static class HtmlUtils 
{
    public static T Color<T>(this T item, Color color) where T : HtmlValueItem
    {
        item.AddAttribute(new Styles.ForeColor(color));
        return item;
    }

    public static T BackColor<T>(this T item, Color color) where T : HtmlItem
    {
        item.AddAttribute(new Styles.BackColor(color));
        return item;
    }

    public static T Fill<T>(this T item) where T : HtmlItem
    {
        item.AddAttribute(new Css.Fill());
        return item;
    }
    
    public static Link GetLinkToTop(string linkText = "back to top") => new (linkText, "#top");
    
    public static Link GetLinkToHtmlItem(this HtmlItem item, string linkText)
    {
        if (string.IsNullOrEmpty(item.Id))
            throw new Exception("To get Html.Item.Link, HtmlItem must have string Id property set.");

        return new Link(linkText, $"#{item.Id}");
    }

    public static T AddWLine<T>(this HtmlItem item, T toAdd) where T : HtmlItem
    {
        item.AddRange(new HtmlItem[] { toAdd, new Break() });
        return toAdd;
    }

    public static T AddOnNewLine<T>(this HtmlItem item, T toAdd) where T : HtmlItem
    {
        item.AddRange(new HtmlItem[] { new Break(),  toAdd });
        return toAdd;
    }

    public static Text Write(this HtmlItem i, string txt) => i.Add(new Text(txt));
    public static Text Write(this HtmlItem i,string txt, Color color) => i.Add(new Text(txt, color));
    public static Text WriteLine(this HtmlItem i, string txt) => i.AddWLine(new Text(txt));
    public static Text WriteLine(this HtmlItem i, string txt, Color color) => i.AddWLine(new Text(txt, color));
}