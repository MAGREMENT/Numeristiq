using Model.Utility;

namespace DesktopApplication.View.Themes.Controls;

public partial class ColorEditorControl
{
    private RGB _color;

    public RGB Color
    {
        get => _color;
        set
        {
            _color = value;
            RedValue.Text = _color.Red.ToString();
            GreenValue.Text = _color.Green.ToString();
            BlueValue.Text = _color.Blue.ToString();
        }
    }
    
    public ColorEditorControl()
    {
        InitializeComponent();
    }

    public void NoColor()
    {
        _color = new RGB();
        RedValue.Text = string.Empty;
        GreenValue.Text = string.Empty;
        BlueValue.Text = string.Empty;
    }
}