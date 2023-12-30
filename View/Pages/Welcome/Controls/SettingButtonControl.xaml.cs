using System.Windows.Controls;
using System.Windows.Media.Imaging;
using View.GlobalControls;
using View.Themes;
using View.Utility;

namespace View.Pages.Welcome.Controls;

public partial class SettingButtonControl : IThemeable
{
    public event OnClick? Click;
    
    public double Size
    {
        set
        {
            Image.Width = value;
            Image.Height = value;
            Text.FontSize = value * 3 / 4;
        }
    }
    
    public SettingButtonControl()
    {
        InitializeComponent();

        MouseLeftButtonDown += (a, o) => Click?.Invoke(a, o);
    }

    public void ApplyTheme(Theme theme)
    {
        Panel.Background = theme.Background1;
        Text.Foreground = theme.Text;
        Image.Source = ImageUtility.SetIconColor((BitmapSource)Image.Source, theme.IconColor);
    }
}