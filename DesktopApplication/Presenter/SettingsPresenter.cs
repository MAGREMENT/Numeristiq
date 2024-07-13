using System.Collections;
using System.Collections.Generic;
using Model.Core.Settings;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter;

public class SettingsPresenter : IEnumerable<NamedListSpan<ISetting>>, ISettingCollection
{
    private readonly Settings _settings;
    private readonly IReadOnlyList<NamedListSpan<ISetting>> _collection;
    private readonly bool _autoUpdate;
    
    public SettingsPresenter(Settings settings, SettingCollections collection, bool autoUpdate = false)
    {
        _settings = settings;
        _collection = _settings.GetCollection(collection);
        _autoUpdate = autoUpdate;
    }

    public void Set(int index, SettingValue value, bool checkValidity)
    {
        _settings.Set(index, value, checkValidity, _autoUpdate);
    }

    public void Update()
    {
        _settings.Update(ListSpan<ISetting>.Merge(_collection));
    }

    public IEnumerator<NamedListSpan<ISetting>> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}