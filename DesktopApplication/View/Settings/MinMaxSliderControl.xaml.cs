using System.Windows;
using DesktopApplication.Presenter;
using Model.Helpers.Settings;
using Model.Utility;

namespace DesktopApplication.View.Settings;

public partial class MinMaxSliderControl
{
    private bool _raiseEvent;
    
    public MinMaxSliderControl(ISettingCollection presenter, IReadOnlySetting setting, int index) : base(presenter, setting, index)
    {
        InitializeComponent();

        var i = (MinMaxSliderInteractionInterface)setting.InteractionInterface;
        Name.Text = setting.Name;
        MinSlider.TickFrequency = i.TickFrequency;
        MaxSlider.TickFrequency = i.TickFrequency;
        MinSlider.Minimum = i.MinMin;
        MinSlider.Maximum = i.MinMax;
        MaxSlider.Minimum = i.MaxMin;
        MaxSlider.Maximum = i.MaxMax;

        var val = setting.Get().ToMinMax();
        _raiseEvent = false;
        MinSlider.Value = val.Min;
        MaxSlider.Value = val.Max;
        _raiseEvent = true;
    }

    public override void Set()
    {
        Set(new MinMaxSettingValue(new MinMax((int)MinSlider.Value, (int)MaxSlider.Value)));
    }

    private void MinChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var old = _raiseEvent;
        _raiseEvent = false;
        if (MaxSlider.Value < MinSlider.Value) MaxSlider.Value = MinSlider.Value;
        _raiseEvent = old;
        
        if(AutoSet && _raiseEvent) Set();
    }

    private void MaxChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var old = _raiseEvent;
        _raiseEvent = false;
        if (MaxSlider.Value < MinSlider.Value) MinSlider.Value = MaxSlider.Value;
        _raiseEvent = old;
        
        if(AutoSet && _raiseEvent) Set();
    }
}