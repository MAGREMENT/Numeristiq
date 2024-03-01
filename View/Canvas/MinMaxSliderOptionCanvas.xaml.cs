using System.Windows;
using Model.Helpers.Settings;
using Model.Utility;

namespace View.Canvas;

public partial class MinMaxSliderOptionCanvas
{
    private readonly SetSetting<MinMax> _setter;
    private readonly GetSetting<MinMax> _getter;
    
    public override string Explanation { get; }
    
    public MinMaxSliderOptionCanvas(string name, string explanation, int minMin, int minMax,
        int maxMin, int maxMax, int tickFrequency, GetSetting<MinMax> getter, SetSetting<MinMax> setter)
    {
        InitializeComponent();
        
        _setter = setter;
        _getter = getter;

        Block.Text = name;
        Explanation = explanation;
        
        MinSlider.Minimum = minMin;
        MinSlider.Maximum = minMax;
        MinSlider.TickFrequency = tickFrequency;
        MaxSlider.Minimum = maxMin;
        MaxSlider.Maximum = maxMax;
        MaxSlider.TickFrequency = tickFrequency;
    }

    
    public override void SetFontSize(int size)
    {
        Block.FontSize = size;
        MinText.FontSize = size;
        MaxText.FontSize = size;
    }

    protected override void InternalRefresh()
    {
        var val = _getter();
        MinSlider.Value = val.Min;
        MaxSlider.Value = val.Max;
    }

    private void OnMinValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!ShouldCallSetter) return;

        if (MinSlider.Value > MaxSlider.Value)
        {
            ShouldCallSetter = false;
            MaxSlider.Value = MinSlider.Value;
            ShouldCallSetter = true;
        }
            
        _setter(new MinMax((int)MinSlider.Value, (int)MaxSlider.Value));
    }
    
    private void OnMaxValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!ShouldCallSetter) return;

        if (MaxSlider.Value < MinSlider.Value)
        {
            ShouldCallSetter = false;
            MinSlider.Value = MaxSlider.Value;
            ShouldCallSetter = true;
        }
            
        _setter(new MinMax((int)MinSlider.Value, (int)MaxSlider.Value));
    }
}