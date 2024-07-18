using System.Windows.Media;

namespace ThemeCreator;

public partial class ColorControl
{ 
    public ColorControl(Brush brush, string name)
    {
        InitializeComponent();

        Color.Background = brush;
        ColorName.Text = name;
    }
}