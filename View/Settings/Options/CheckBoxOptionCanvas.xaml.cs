using System.Windows;

namespace View.Settings.Options;

public partial class CheckBoxOptionCanvas
{
    private readonly bool _callOnChange;
    private readonly OnChange<bool> _onChange;

    public CheckBoxOptionCanvas(string text, string explanation, bool isChecked, OnChange<bool> onChange)
    {
        InitializeComponent();
        
        _onChange = onChange;
        
        _callOnChange = false;
        Box.IsChecked = isChecked;
        _callOnChange = true;
        
        Box.Content = text;

        Explanation = explanation;
    }

    private void OnChecked(object sender, RoutedEventArgs e)
    {
        ChangeEvent();
    }

    private void OnUnchecked(object sender, RoutedEventArgs e)
    {
        ChangeEvent();
    }

    private void ChangeEvent()
    {
        if (!_callOnChange) return;
        
        var val = Box.IsChecked;
        if (val is null) return;

        _onChange(val.Value);
    }

    public override string Explanation { get; }
}

