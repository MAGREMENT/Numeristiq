using System.Windows.Media;
using Presenter;
using View.Utility;

namespace View.Themes;

public class Theme
{
    private Theme(Color background1, Color background2, Color background3, Color primary1, Color primary2,
        Color secondary1, Color secondary2, Color accent, Color text)
    {
        Background1 = background1;
        Background2 = background2;
        Background3 = background3;
        Primary1 = primary1;
        Primary2 = primary2;
        Secondary1 = secondary1;
        Secondary2 = secondary2;
        Accent = accent;
        Text = text;
    }

    public static Theme From(ThemeDAO dao)
    {
        return new Theme(ColorManager.ToColor(dao.Background1), ColorManager.ToColor(dao.Background2),
            ColorManager.ToColor(dao.Background3), ColorManager.ToColor(dao.Primary1),
            ColorManager.ToColor(dao.Primary2), ColorManager.ToColor(dao.Secondary1),
            ColorManager.ToColor(dao.Secondary2), ColorManager.ToColor(dao.Accent),
            ColorManager.ToColor(dao.Text));
    }

    public Color Background1 { get; }
    public Color Background2 { get; }
    public Color Background3 { get; }
    public Color Primary1 { get; }
    public Color Primary2 { get; }
    public Color Secondary1 { get; }
    public Color Secondary2 { get; }
    public Color Accent { get; }
    public Color Text { get; }
}