using System.Collections.Generic;
using Model;
using Model.Helpers.Settings;
using Model.Helpers.Settings.Types;
using Model.Utility;

namespace DesktopApplication.Presenter;

public class Settings : UpdateCallback
{
    private readonly ISetting[] _settings;
    private readonly IRepository<Dictionary<string, SettingValue>> _repository;

    public Settings(Theme[] themes, IRepository<Dictionary<string, SettingValue>> repository)
    {
        _settings = new ISetting[] { new IntSetting("Theme", new NameListInteractionInterface(themes), -1) };
        _repository = repository;
        
        WelcomeWindowSettings = new SettingsSpan(new []{
                new NamedListSpan<ISetting>("Themes", _settings, 0)
            }, this);
    }

    public void TrySet(string name, SettingValue value)
    {
        foreach (var setting in _settings)
        {
            if (setting.Name.Equals(name))
            {
                setting.Set(value);
                return;
            }
        }
    }
    
    public void Update()
    {
        Dictionary<string, SettingValue> toUpload = new();

        foreach (var setting in _settings)
        {
            toUpload.Add(setting.Name, setting.Get());
        }
        
        _repository.Upload(toUpload);
    }

    public SettingsSpan WelcomeWindowSettings { get; }

    public ISetting Theme => _settings[0];
    
    
}