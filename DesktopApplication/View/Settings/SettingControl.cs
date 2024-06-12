using System.Windows.Controls;
using DesktopApplication.Presenter;
using Model.Core.Settings;

namespace DesktopApplication.View.Settings;

public abstract class SettingControl : UserControl
{
    private readonly ISettingCollection _callback;
    private readonly int _index;
    
    public bool AutoSet { get; set; }
    public abstract void Set();
    
    protected IReadOnlySetting Setting { get; }

    protected void Set(SettingValue value)
    {
        _callback.Set(_index, value, false);
    }

    protected SettingControl(ISettingCollection callback, IReadOnlySetting setting, int index)
    {
        _callback = callback;
        _index = index;
        Setting = setting;
    }
}