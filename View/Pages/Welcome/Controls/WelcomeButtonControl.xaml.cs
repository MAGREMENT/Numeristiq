using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using View.GlobalControls;
using View.Themes;
using View.Utility;

namespace View.Pages.Welcome.Controls;

public partial class WelcomeButtonControl
{
    public event OnClick? Click;

    private Brush _defaultBorder = Brushes.Black;
    private Brush _hoverBorder = Brushes.MediumPurple;

    public string TitleText
    {
        set => Title.Text = value;
    }

    public string DescriptionText
    {
        set => Description.Text = value;
    }

    public BitmapImage Icon
    {
        set => Image.Source = value;
    }
    
    public WelcomeButtonControl()
    {
        InitializeComponent();

        MouseLeftButtonDown += (o, a) => Click?.Invoke(o, a);
        MouseEnter += (_, _) => Border.BorderBrush = _hoverBorder;
        MouseLeave += (_, _) => Border.BorderBrush = _defaultBorder;
        
        ((App)Application.Current).ThemeChanged += ApplyTheme;
    }

    private void ApplyTheme(Theme theme)
    {
        _defaultBorder = theme.Background2;
        _hoverBorder = theme.Primary1;
        Image.Source = ImageUtility.SetIconColor((BitmapSource)Image.Source, theme.IconColor);
    }
}

