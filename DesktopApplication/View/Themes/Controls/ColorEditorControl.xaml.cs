using System.Windows.Controls;
using Model.Utility;

namespace DesktopApplication.View.Themes.Controls;

public partial class ColorEditorControl
{
    private RGB _color;
    private bool _fireEvent;

    public event OnColorChange? ColorChanged;

    public RGB Color
    {
        get => _color;
        set
        {
            _fireEvent = false;
            _color = value;
            
            RedValue.Text = _color.Red.ToString();
            GreenValue.Text = _color.Green.ToString();
            BlueValue.Text = _color.Blue.ToString();

            _fireEvent = true;
        }
    }
    
    public ColorEditorControl()
    {
        InitializeComponent();
    }

    public void NoColor()
    {
        _fireEvent = false;
        _color = new RGB();
        
        RedValue.Text = string.Empty;
        GreenValue.Text = string.Empty;
        BlueValue.Text = string.Empty;

        _fireEvent = true;
    }

    private void ColorValueChanged(object sender, TextChangedEventArgs e)
    {
        if (!_fireEvent || sender is not TextBox box) return;

        if (!byte.TryParse(box.Text, out _))
        {
            _fireEvent = false;
            box.Text = box.Name switch
            {
                nameof(RedValue) => _color.Red.ToString(),
                nameof(GreenValue) => _color.Green.ToString(),
                _ => _color.Blue.ToString()
            };
            _fireEvent = true;
            return;
        }

        _color = new RGB(byte.Parse(RedValue.Text), byte.Parse(GreenValue.Text),
            byte.Parse(BlueValue.Text));
        ColorChanged?.Invoke(_color);
    }
}

public delegate void OnColorChange(RGB color);