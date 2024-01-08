using System.Windows.Media;
using Global.Enums;
using Presenter.Translators;
using View.Utility;

namespace View.Themes;

public class Theme
{
    private Theme(Brush background1, Brush background2, Brush background3, Brush primary1, Brush primary2,
        Brush secondary1, Brush secondary2, Brush accent, Brush border, Brush text, IconColor iconColor)
    {
        Background1 = background1;
        Background2 = background2;
        Background3 = background3;
        Primary1 = primary1;
        Primary2 = primary2;
        Secondary1 = secondary1;
        Secondary2 = secondary2;
        Accent = accent;
        Border = border;
        Text = text;
        IconColor = iconColor;
    }

    public static Theme From(ViewTheme dao)
    {
        return new Theme(ColorUtility.ToBrush(dao.Background1), ColorUtility.ToBrush(dao.Background2),
            ColorUtility.ToBrush(dao.Background3), ColorUtility.ToBrush(dao.Primary1),
            ColorUtility.ToBrush(dao.Primary2), ColorUtility.ToBrush(dao.Secondary1),
            ColorUtility.ToBrush(dao.Secondary2), ColorUtility.ToBrush(dao.Accent),
            ColorUtility.ToBrush(dao.Border), ColorUtility.ToBrush(dao.Text), dao.IconColor);
    }

    public Brush Background1 { get; }
    public Brush Background2 { get; }
    public Brush Background3 { get; }
    public Brush Primary1 { get; }
    public Brush Primary2 { get; }
    public Brush Secondary1 { get; }
    public Brush Secondary2 { get; }
    public Brush Accent { get; }
    public Brush Text { get; }
    public Brush Border { get; }
    public IconColor IconColor { get; }
}

public delegate void ApplyTheme(Theme theme);