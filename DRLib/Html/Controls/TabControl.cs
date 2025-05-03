using System.Drawing;
using DRLib.Html.Core;
using DRLib.Html.Css;
using DRLib.Html.Scripts;

namespace DRLib.Html.Controls;

public sealed record TabControl : HtmlItem
{
    public const string TAB_OPEN = "openTab";
    public const string TAB_PANE = "tab-pane";
    public const string TAB_BUTTON = "tab-button";

    private bool HasActive;
    private readonly Html.Items.Div TabButtons;
    private readonly Html.Items.Div TabContent;

    public TabControl(string id, Color baseTabColor, bool enableKeys = false) :
        this(id, new TabColorInfo(baseTabColor), enableKeys)
    { }

    public TabControl(string id, TabColorInfo color = null, bool enableKeys = false) : base("tabPage")
    {
        color ??= new TabColorInfo();

        Id = id;
        TabButtons = Add(new Html.Items.Div {
            InitAttribute = new HtmlClass($"{Id}-tabs",
                new HtmlStyle("position", "sticky"),
                new HtmlStyle("top", "0"),
                new HtmlStyle("z-index", "1000"), /* Ensure it stays on top of other content */
                new HtmlStyle("overflow", "hidden"),
                new Html.Styles.BackColor(color.TabColor)
            )
        });
        TabContent = Add(new Html.Items.Div {
            InitAttribute = new HtmlClass($"{Id}-tab-content",
                new HtmlStyle("border", "1px solid #ccc"),
                new HtmlStyle("border-top", "none")
            )
        });
        Add(new OpenTabScript(Id, enableKeys));
        AddAttribute(GetClass(new DefaultTabButtonClass()));
        AddAttribute(GetClass(new HtmlClass($"{TAB_BUTTON}.active", new Html.Styles.BackColor(color.ActiveColor))));
        AddAttribute(GetClass(new HtmlClass($"{TAB_BUTTON}:hover", new Html.Styles.BackColor(color.HoverColor))));
    }

    private HtmlClass GetClass(HtmlClass htmlClass) => htmlClass with {
        ClassName = $"{Id}-{htmlClass.ClassName}",
        Selector = ClassSelector.ClassName,
    };

    public Html.Items.Div AddTab(string tabText, bool isActive = false) => AddTab(new Html.Items.Text(tabText), isActive);

    public Html.Items.Div AddTab(HtmlItem tabText, bool isActive = false)
    {
        HasActive &= isActive;
        var tabId = $"{Id}-Tab{TabButtons.GetAllItems().Length + 1}";

        var tabButton = TabButtons.Add(new HtmlTabButton(tabText));
        var buttonStyle = isActive ? $"{TAB_BUTTON} active" : TAB_BUTTON;
        tabButton.AddAttribute(new HtmlAttribute("class", $"{Id}-{buttonStyle}"));
        tabButton.AddAttribute(new HtmlAttribute("onclick", $"{TAB_OPEN}_{Id}(event, '{tabId}');"));

        var tab = TabContent.Add(new Html.Items.Div() { InitAttribute = new HtmlAttribute("id", tabId) });
        tab.AddAttribute(new HtmlClass($"{Id}-{TAB_PANE}", new Html.Styles.Display("none")));
        if (isActive && !HasActive)
            tab.AddAttribute(new HtmlStyle("display", "block"));

        return tab;
    }

    public class TabColorInfo(Color? _tabColor = null)
    {
        public Color TabColor { get; init; } = _tabColor ?? Color.Gainsboro;

        private Color? _hoverColor;
        public Color HoverColor
        {
            get => _hoverColor ?? GetDarkerShade(TabColor, 0.8);
            set => _hoverColor = value;
        }

        private Color? _activeColor;
        public Color ActiveColor
        {
            get => _activeColor ?? GetDarkerShade(TabColor, 0.6);
            set => _activeColor = value;
        }

        public static Color GetDarkerShade(Color color, double factor)
        {
            var r = (int)(color.R * factor);
            var g = (int)(color.G * factor);
            var b = (int)(color.B * factor);

            return Color.FromArgb(color.A, r, g, b);
        }
    }

    public record DefaultTabButtonClass() : HtmlClass(TabControl.TAB_BUTTON,
        new HtmlStyle("background-color", "inherit"),
        new HtmlStyle("float", "left"),
        new HtmlStyle("border", "none"),
        new HtmlStyle("outline", "none"),
        new HtmlStyle("cursor", "pointer"),
        new HtmlStyle("padding", "14px 16px"),
        new HtmlStyle("transition", "0.3s")
    );

    public sealed record HtmlTabButton : HtmlItem
    {
        public HtmlTabButton(HtmlItem Text) : base("button")
        {
            Add(Text);
        }
    }

    public record OpenTabScript(string TabId, bool EnableArrowKeyScroll) : HtmlJScript
    {
        protected override string GetScriptText()
        {
            var text = """
           function openTab(event, tabName) {
               var i, tabPanes, tabButtons;
               tabPanes = document.getElementsByClassName("tab-pane");
               for (i = 0; i < tabPanes.length; i++) {
                   tabPanes[i].style.display = "none";
                   tabPanes[i].classList.remove("active");
               }
               tabButtons = document.getElementsByClassName("tab-button");
               for (i = 0; i < tabButtons.length; i++) {
                   tabButtons[i].className = tabButtons[i].className.replace(" active", "");
               }
               // Show the current tab and add an "active" class to the button that opened the tab
               document.getElementById(tabName).style.display = "block";
               document.getElementById(tabName).classList.add("active");
               event.currentTarget.className += " active";
           }
           """;

            if (EnableArrowKeyScroll)
                text += """
                document.addEventListener('keydown', function(event) {
                    const activeButton = document.querySelector('.tab-button.active');
                    const buttons = Array.from(document.querySelectorAll('.tab-button'));
                    let index = buttons.indexOf(activeButton);
                
                    if (event.key === 'ArrowRight') {
                        index = (index + 1) % buttons.length; // Move to the next tab
                    } else if (event.key === 'ArrowLeft') {
                        index = (index - 1 + buttons.length) % buttons.length; // Move to the previous tab
                    } else {
                        return; // Exit if it's not an arrow key
                    }
                
                    buttons[index].click(); // Simulate a click on the next or previous tab button
                })
                """;
            text = text.Replace(TabControl.TAB_OPEN, $"{TabControl.TAB_OPEN}_{TabId}");
            text = text.Replace(TabControl.TAB_PANE, $"{TabId}-{TabControl.TAB_PANE}");
            text = text.Replace(TabControl.TAB_BUTTON, $"{TabId}-{TabControl.TAB_BUTTON}");

            return text;
        }
    }
}