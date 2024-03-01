using System.Windows;
using Model.Helpers.Settings;
using View.Themes;

namespace View.Canvas;

public partial class SliderOptionCanvas
{
    private readonly SetSetting<int> _setter;
    private readonly GetSetting<int> _getter;
    
    public SliderOptionCanvas(string name, string explanation, int min, int max, int tickFrequency, GetSetting<int> getter, SetSetting<int> setter)
    {
        InitializeComponent();

        Block.Text = name;
        
        _setter = setter;
        _getter = getter;

        Slider.Maximum = max;
        Slider.Minimum = min;
        Slider.TickFrequency = tickFrequency;

        Explanation = explanation;
    }

    public override string Explanation { get; }
    public override void SetFontSize(int size)
    {
        Block.FontSize = size;
    }

    protected override void InternalRefresh()
    {
        Slider.Value = _getter();
    }

    private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ShouldCallSetter) _setter((int)Slider.Value);
    }
}