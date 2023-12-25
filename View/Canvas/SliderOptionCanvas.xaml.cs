using System.Windows;

namespace View.Canvas;

public partial class SliderOptionCanvas
{
    private readonly OnChange<int> _onChange;
    private readonly bool _callOnChange;
    
    public SliderOptionCanvas(string name, string explanation, int min, int max, int tickFrequency, int defaultValue, OnChange<int> onChange)
    {
        InitializeComponent();

        Block.Text = name;
        
        _onChange = onChange;

        Slider.Maximum = max;
        Slider.Minimum = min;
        Slider.TickFrequency = tickFrequency;
        
        _callOnChange = false;
        Slider.Value = defaultValue;
        _callOnChange = true;

        Explanation = explanation;
    }

    public override string Explanation { get; }
    public override void SetFontSize(int size)
    {
        Block.FontSize = size;
    }

    private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_callOnChange) _onChange((int)Slider.Value);
    }
}