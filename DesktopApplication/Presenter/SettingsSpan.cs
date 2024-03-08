using System.Collections;
using System.Collections.Generic;
using Model.Helpers.Settings;
using Model.Utility;

namespace DesktopApplication.Presenter;

public class SettingsSpan : IEnumerable<NamedListSpan<ISetting>>
{
    private readonly IReadOnlyList<NamedListSpan<ISetting>> _settings;
    private readonly UpdateCallback _callback;

    public SettingsSpan(IReadOnlyList<NamedListSpan<ISetting>> settings, UpdateCallback callback)
    {
        _settings = settings;
        _callback = callback;
    }

    public void Update()
    {
        _callback.Update();
    }

    public IEnumerator<NamedListSpan<ISetting>> GetEnumerator()
    {
        return _settings.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public interface UpdateCallback
{
    public void Update();
}