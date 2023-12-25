using System.Windows;
using Global;

namespace View.Canvas;

public partial class MinMaxSliderOptionCanvas
{
    private readonly OnChange<MinMax> _onChange;
    private bool _callOnChange;
    
    public override string Explanation { get; }
    
    public MinMaxSliderOptionCanvas(string name, string explanation, int minMin, int minMax,
        int maxMin, int maxMax, int tickFrequency, MinMax defaultValue, OnChange<MinMax> onChange)
    {
        InitializeComponent();

        Block.Text = name;
        Explanation = explanation;
        _onChange = onChange;
        
        MinSlider.Minimum = minMin;
        MinSlider.Maximum = minMax;
        MinSlider.TickFrequency = tickFrequency;
        MaxSlider.Minimum = maxMin;
        MaxSlider.Maximum = maxMax;
        MaxSlider.TickFrequency = tickFrequency;

        _callOnChange = false;
        MinSlider.Value = defaultValue.Min;
        MaxSlider.Value = defaultValue.Max;
        _callOnChange = true;
    }

    
    public override void SetFontSize(int size)
    {
        Block.FontSize = size;
        MinText.FontSize = size;
        MaxText.FontSize = size;
    }

    private void OnMinValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_callOnChange) return;

        if (MinSlider.Value > MaxSlider.Value)
        {
            _callOnChange = false;
            MaxSlider.Value = MinSlider.Value;
            _callOnChange = true;
        }
            
        _onChange(new MinMax((int)MinSlider.Value, (int)MaxSlider.Value));
    }
    
    private void OnMaxValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_callOnChange) return;

        if (MaxSlider.Value < MinSlider.Value)
        {
            _callOnChange = false;
            MinSlider.Value = MaxSlider.Value;
            _callOnChange = true;
        }
            
        _onChange(new MinMax((int)MinSlider.Value, (int)MaxSlider.Value));
    }
}