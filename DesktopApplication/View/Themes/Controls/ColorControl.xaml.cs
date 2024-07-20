using System.Windows.Media;

namespace DesktopApplication.View.Themes.Controls;

public partial class ColorControl
{ 
    public ColorControl(Brush brush, string name)
    {
        InitializeComponent();

        Color.Background = brush;
        ColorName.Text = name;
    }
}