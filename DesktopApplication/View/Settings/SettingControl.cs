using System.Windows.Controls;
using Model.Core.Settings;

namespace DesktopApplication.View.Settings;

public abstract class SettingControl : UserControl
{
    private readonly ISettingCollection _callback;
    private readonly int _index;
    
    public bool AutoSet { get; set; }
    public abstract void Set();

    protected void Set(SettingValue value)
    {
        _callback.Set(_index, value, false);
    }

    protected SettingControl(ISettingCollection callback, int index)
    {
        _callback = callback;
        _index = index;
    }
}