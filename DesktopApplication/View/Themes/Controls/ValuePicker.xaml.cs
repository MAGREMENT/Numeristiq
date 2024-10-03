using System;
using System.Windows;
using System.Windows.Controls;

namespace DesktopApplication.View.Themes.Controls;

public partial class ValuePicker
{
    private bool _fireEvent = true;
    private int _current;
    private int _min;
    private int _max = int.MaxValue;
    
    public string Title
    {
        set => TitleTextBlock.Text = value;
    }

    public int Min
    {
        set
        {
            _min = value;
            if (_current < _min) _current = _min;
        }
    }

    public int Max
    {
        set
        {
            _max = value;
            if (_current > _max) _current = _max;
        }
    }

    public event OnValuePicked? ValuePicked;
    
    public ValuePicker()
    {
        InitializeComponent();
    }

    private void OnValueChange(object sender, TextChangedEventArgs e)
    {
        if (!_fireEvent || sender is not TextBox box) return;
        
        if (string.IsNullOrWhiteSpace(box.Text)) _current = _min;
        else
        {
            if (int.TryParse(box.Text, out var i)) _current = Math.Max(_min, Math.Min(_max, i));

            _fireEvent = false;
            box.Text = _current.ToString();
            _fireEvent = true;
        }

        UpButton.IsEnabled = _current < _max;
        DownButton.IsEnabled = _current > _max;

        ValuePicked?.Invoke(_current);
    }

    public void SetCurrent(int n)
    {
        _current = n;
        ActualValue.Text = _current.ToString();
    }

    private void UpValue(object sender, RoutedEventArgs e)
    {
        SetCurrent(_current + 1);
    }

    private void DownValue(object sender, RoutedEventArgs e)
    {
        SetCurrent(_current - 1);
    }
}

public delegate void OnValuePicked(int n);