using DRLib.Html.Core;
using DRLib.Html.Items;
using DRLib.Html.Rendering;

namespace DRLib.Html;

[Flags]
public enum RenderOptions
{
    Null = 0,
    SingleFile = 1 << 1,
    MultiFile = 2 << 1,
    WithoutCss = 3 << 1,
}

/// <summary>
/// Html builder used to create formatted HTML documents with Head, Body and Style sections
/// </summary>
public class HtmlBuilder
{
    private readonly Doc Html = new ();
    private Head Head => Html.Head;
    private Body Body => Html.Body;
    private readonly HashSet<Type> Unique = new () { typeof(Doc), typeof(Head), typeof(Body) };

    public T AddToHead<T>(T item) where T : HtmlItem
    {
        if (item is IHtmlUnique && Unique.Contains(item.GetType()))
            throw new Exception($"Html can only contain single instance of {item.GetType().Name} <{item.Tag}>");
        if (item is IHtmlUnique)
            Unique.Add(item.GetType());

        if(item.Tag is not ("title" or "style" or "meta" or "link" or "script" or "base"))
            throw new Exception($"Html head can only contain title, style, meta, link, script, and base tags. {item.Tag} is not allowed.");

        return Head.Add(item);
    }

    public T AddOnNewLine<T>(T item) where T : HtmlItem
    {
        Body.Add(new Break());
        var ret = Add(item);
        return ret;
    }
    
    public T AddWLine<T>(T item) where T : HtmlItem
    {
        var ret = Add(item);
        Body.Add(new Break());
        return ret;
    }

    public T Add<T>(T item) where T : HtmlItem
    {
        if (item is IHtmlUnique && Unique.Contains(item.GetType()))
            throw new Exception($"Html can only contain single instance of {item.GetType().Name} <{item.Tag}>");
        if (item is IHtmlUnique)
            Unique.Add(item.GetType());

        return Body.Add(item);
    }

    public void AddBodyAttributes(params HtmlAttribute[] attr) => Body.AddAttributes(attr);
    public void AddHeadAttributes(params HtmlAttribute[] attr) => Body.AddAttributes(attr);

    public string RenderHtml(bool withCSS = true) => withCSS ? Html.RenderHtml() : Html.RenderWithoutCss();
    public string RenderBody() => Body.RenderWithoutCss();

    public override string ToString() => $"{GetSummary(Html)}{string.Join("\n", Html.GetAllItems().Select(GetSummary))}";
    private static string GetSummary(HtmlItem i) => 
        $"{i.GetType().Name} [{i.Tag}] {{\n\t" +
        $"{string.Join("\n\t", i.GetAllItems().Where(r => r.Tag is not ("br" or "span")).Select(r => $"{r.GetType().Name} [{r.Tag}]: {r.GetAllItems().Length}"))}\n}}\n";

    // convenience methods
    public void SetDocumentTitle(string title) => AddToHead(new Title(title));

    public void UseUnicodeEncoding() => AddToHead(new Meta {
        InitAttribute = new HtmlAttribute("charset", "utf-8")
    });
}
