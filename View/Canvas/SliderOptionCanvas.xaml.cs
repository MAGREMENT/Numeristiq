using System.Windows;
using Global;
using View.Themes;

namespace View.Canvas;

public partial class SliderOptionCanvas
{
    private readonly SetArgument<int> _setter;
    private readonly GetArgument<int> _getter;
    
    public SliderOptionCanvas(string name, string explanation, int min, int max, int tickFrequency, GetArgument<int> getter, SetArgument<int> setter)
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