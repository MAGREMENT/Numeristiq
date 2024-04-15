using System.Windows;
using DesktopApplication.Presenter;
using Model.Helpers.Settings;

namespace DesktopApplication.View.Settings;

public partial class SliderControl
{
    private readonly bool _raiseEvent;
    
    public SliderControl(ISettingCollection presenter, IReadOnlySetting setting, int index) : base(presenter, setting, index)
    {
        InitializeComponent();

        var i = (SliderInteractionInterface)setting.InteractionInterface;

        Name.Text = setting.Name;
        Slider.TickFrequency = i.TickFrequency;
        Slider.Minimum = i.Min;
        Slider.Maximum = i.Max;

        _raiseEvent = false;
        Slider.Value = setting.Get().ToInt();
        _raiseEvent = true;
    }

    public override void Set()
    {
        Set(new DoubleSettingValue(Slider.Value));
    }

    private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (AutoSet && _raiseEvent) Set();
    }
}