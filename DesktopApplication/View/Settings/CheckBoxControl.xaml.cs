using System.Windows;
using DesktopApplication.Presenter;
using Model.Core.Settings;

namespace DesktopApplication.View.Settings;

public partial class CheckBoxControl
{
    private readonly bool _raiseEvent;
    
    public CheckBoxControl(ISettingCollection presenter, IReadOnlySetting setting, int index) : base(presenter, setting, index)
    {
        InitializeComponent();

        Box.Content = setting.Name;

        _raiseEvent = false;
        Box.IsChecked = setting.Get().ToBool();
        _raiseEvent = true;
    }

    public override void Set()
    {
        var b = Box.IsChecked;
        if (b is null) return;

        Set(new BoolSettingValue(b.Value));
    }

    private void OnChecked(object sender, RoutedEventArgs e)
    {
        if (AutoSet && _raiseEvent) Set();
    }

    private void OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (AutoSet && _raiseEvent) Set();
    }
}