using System.Windows;

namespace View.HelperWindows.Settings.Options;

public partial class SliderOptionCanvas : OptionCanvas
{
    
    private readonly OnChange<int> _onChange;
    private readonly bool _callOnChange;
    
    public SliderOptionCanvas(string name, string explanation, int max, int min, int defaultValue, OnChange<int> onChange)
    {
        InitializeComponent();

        Block.Text = name;
        
        _onChange = onChange;

        Slider.Maximum = max;
        Slider.Minimum = min;
        _callOnChange = false;
        Slider.Value = defaultValue;
        _callOnChange = true;

        Explanation = explanation;
    }

    public override string Explanation { get; }

    private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_callOnChange) _onChange((int)Slider.Value);
    }
}