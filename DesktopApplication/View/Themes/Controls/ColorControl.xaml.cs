using System.Windows.Media;

namespace DesktopApplication.View.Themes.Controls;

public partial class ColorControl
{ 
    public ColorControl(Brush brush, string name, bool canBeSelected)
    {
        InitializeComponent();

        Color.Background = brush;
        ColorName.Text = name;

        if (canBeSelected)
        {
            MouseEnter += (_, _) => SetResourceReference(BackgroundProperty, "Background3");
            MouseLeave += (_, _) => SetResourceReference(BackgroundProperty, "Background2");
        }

        IsEnabled = canBeSelected;
    }
}