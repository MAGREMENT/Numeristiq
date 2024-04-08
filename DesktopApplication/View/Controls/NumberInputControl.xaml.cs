using System.Windows;

namespace DesktopApplication.View.Controls;

public partial class NumberInputControl
{
    private int _min;
    private int _max = 1;
    private int _value;

    public event OnValueChange? ValueChanged;

    public int Min
    {
        get => _min;
        set
        {
            _min = value > _max ? _max : value;
            if (_value < _min)
            {
                _value = _min;
                OnValueChanged();
            }
        } 
    }

    public int Max
    {
        get => _max;
        set
        {
            _max = value < _min ? _min : value;
            if (_value > _max)
            {
                _value = _max;
                OnValueChanged();
            }
        } 
    }
    
    public NumberInputControl()
    {
        InitializeComponent();
    }

    private void Up(object sender, RoutedEventArgs e)
    {
        if (_value < _max)
        {
            _value++;
            OnValueChanged();
        }
    }
    
    private void Down(object sender, RoutedEventArgs e)
    {
        if (_value > _min)
        {
            _value--;
            OnValueChanged();
        }
    }

    private void OnValueChanged()
    {
        Number.Text = _value.ToString();
        ValueChanged?.Invoke(_value);
    }
}

public delegate void OnValueChange(int value);