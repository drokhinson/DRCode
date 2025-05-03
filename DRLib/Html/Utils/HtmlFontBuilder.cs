using System.Drawing;
using DRLib.Html.Core;
using DRLib.Html.Css;

namespace DRLib.Html.Utils;

/// <summary>
/// Allows user to build a font style
/// </summary>
public sealed record HtmlFontBuilder
{
    public Styles.TextAlign Align { get; set; }
    public Styles.FontFamily Font { get; set; }
    public Styles.FontSize FontSize { get; set; }
    public Styles.FontWeight FontWeight { get; set; }

    private Styles.ForeColor _color;
    public Color Color { set => _color = new Styles.ForeColor(value); }

    private Styles.BackColor _highlight;
    public Color Highlight { set => _highlight = new Styles.BackColor(value); }

    private Styles.ItalicFont _italic;
    public bool Italic { set => _italic = value ? new Styles.ItalicFont() : null; }

    private Styles.BoldFont _bold;
    public bool Bold { set => _bold = value ? new Styles.BoldFont() : null; }

    public HtmlStyle[] GetStyles() => new HtmlStyle[]{
        Align, _color, _highlight, Font, FontSize, _italic, _bold, FontWeight
    }.Where(r => r is not null).ToArray();

    public HtmlClass GetClassAttribute(string fontName) => new(fontName, GetStyles());
}