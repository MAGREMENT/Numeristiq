using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using View.GlobalControls;
using View.Themes;
using View.Utility;

namespace View.Pages.Welcome.Controls;

public partial class SettingButtonControl
{
    public event OnClick? Click;

    private Brush _background;
    private Brush _hoverBackground;
    
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

        _background = Panel.Background;
        _hoverBackground = Panel.Background;
        
        MouseLeftButtonDown += (a, o) => Click?.Invoke(a, o);
        MouseEnter += (_, _) => Panel.Background = _hoverBackground;
        MouseLeave += (_, _) => Panel.Background = _background;

        ((App)Application.Current).ThemeChanged += ApplyTheme;
    }

    private void ApplyTheme(Theme theme)
    {
        _background = theme.Background1;
        _hoverBackground = theme.Background3;
        Image.Source = ImageUtility.SetIconColor((BitmapSource)Image.Source, theme.IconColor);
    }
}