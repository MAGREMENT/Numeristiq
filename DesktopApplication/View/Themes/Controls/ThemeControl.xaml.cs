using System.Windows.Media;

namespace DesktopApplication.View.Themes.Controls;

public partial class ThemeControl
{
    public ThemeControl(string name, bool editable)
    {
        InitializeComponent();

        ThemeName.Text = name;
        if (!editable)
        {
            EditableBorder.Background = new SolidColorBrush
            {
                Color = (Color)FindResource("OffColor"),
                Opacity = 0.25
            };

            EditableBlock.Text = "Uneditable";
            EditableBlock.SetResourceReference(ForegroundProperty, "Off");
        }

        MouseEnter += (_, _) => SetResourceReference(BackgroundProperty, "BackgroundHighlighted");
        MouseLeave += (_, _) => SetResourceReference(BackgroundProperty, "Background1");
    }
}