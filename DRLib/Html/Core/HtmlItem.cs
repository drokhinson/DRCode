global using HtmlSize = OneOf.OneOf<int, double, string>;
using System.Xml.Linq;

namespace DRLib.Html.Core;

/// <summary> Core HtmlItem containing a list of HtmlItems and Attributes </summary>
public abstract partial record HtmlItem(string Tag)
{
    protected HtmlItem(string tag, params HtmlAttribute[] attributes) : this(tag) => Attributes.AddRange(attributes);

    private readonly List<HtmlItem> Items = new();
    private readonly List<HtmlAttribute> Attributes = new();

    /// <summary>
    /// Text that will be displayed before any contained items. <br></br>
    /// No formatting or attributes will be applied to this text.
    /// </summary>
    public virtual string TextValue { get; set; } = string.Empty;

    /// <summary>
    /// If true, html characters will be replaced with literal characters.<br></br>
    /// If false, html characters will remain unmodified.
    /// </summary>
    public virtual bool ConvertHtmlCharacters { get; set; } = true;

    /// <summary> Setter overrides Adds or Replaces any ID attributes. </summary>
    public string Id
    {
        get => Attributes.SingleOrDefault(r => r.Name.ToUpper() == "ID")?.Text;
        set => SetAttribute(new HtmlAttribute("id", value));
    }
    
    public HtmlAttribute InitAttribute { init => AddAttribute(value); }
    public HtmlAttribute[] InitAttributes { init => AddAttributes(value); }
    
    public XElement ToXElement()
    {
        PreRenderActions();

        var e = new XElement(Tag);

        if (TextValue != null && ConvertHtmlCharacters)
            e.Value = TextValue;
        else if (TextValue != null)
            e.Add(new XCData(TextValue)); // XCData keeps xml characters unmodified

        var attributes = Attributes
            .GroupBy(r => r.Name)
            .Select(grp => new XAttribute(grp.Key, string.Join(' ', grp.Distinct().Select(r => r.Text))));

        foreach (var attr in attributes)
            e.Add(attr);

        foreach (var i in Items)
            e.Add(i.ToXElement());

        return e;
    }

    /// <summary> Override to apply custom actions that occur before rending XElement </summary>
    protected virtual void PreRenderActions() { }
}

#region Modify and Get objects in Item/Attribute Lists

public abstract partial record HtmlItem
{
    public virtual T Add<T>(T item) where T : HtmlItem
    {
        Items.Add(item);
        return item;
    }

    public virtual void AddRange<T>(IEnumerable<T> items) where T : HtmlItem => Items.AddRange(items);

    public virtual T Insert<T>(int index, T item) where T : HtmlItem
    {
        Items.Insert(index, item);
        return item;
    }

    public void RemoveAll(Predicate<HtmlItem> predicate) => Items.RemoveAll(predicate);

    public T AddAttribute<T>(T item) where T : HtmlAttribute
    {
        Attributes.Add(item);
        return item;
    }
    
    public void AddAttributes<T>(IEnumerable<T> attributes) where T : HtmlAttribute 
        => Attributes.AddRange(attributes);
    
    public void InsertAttributes<T>(int index, IEnumerable<T> attributes) where T : HtmlAttribute 
        => Attributes.InsertRange(index, attributes);
    
    public void RemoveAttribute(HtmlAttribute attribute) => Attributes.Remove(attribute);
    
    public void RemoveAllAttributes(Predicate<HtmlAttribute> predicate) => Attributes.RemoveAll(predicate);

    /// <summary> Adds 'attribute' to attribute list, removing any previously added attributes with the same tag </summary>
    public T SetAttribute<T>(T attribute) where T : HtmlAttribute
    {
        Attributes.RemoveAll(r => string.Equals(r.Name, attribute.Name, StringComparison.CurrentCultureIgnoreCase));
        Attributes.Add(attribute);
        return attribute;
    }
    
    public HtmlItem[] GetAllItems() => Items.ToArray();
    public T[] GetItemsOfType<T>() where T : HtmlItem => Items.OfType<T>().ToArray();
    public T[] GetAttributesOfType<T>() where T : HtmlAttribute => Attributes.OfType<T>().ToArray();
}

#endregion
