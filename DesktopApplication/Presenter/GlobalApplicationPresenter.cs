using System;
using Model;
using Repository;

namespace DesktopApplication.Presenter;

public class GlobalApplicationPresenter
{
    private readonly Settings _settings;
    
    public Theme? Theme { get; private set; }

    private GlobalApplicationPresenter(Settings settings, Theme? theme)
    {
        _settings = settings;
        Theme = theme;
    }

    public WelcomePresenter Initialize()
    {
        return new WelcomePresenter(_settings);
    }
    
    private static GlobalApplicationPresenter? _instance;

    public static GlobalApplicationPresenter Instance
    {
        get
        {
            _instance ??= InitializeInstance();
            return _instance;
        }
    }

    private static GlobalApplicationPresenter InitializeInstance()
    {
        var themeRepository = new HardCodedThemeRepository();
        if (!themeRepository.Initialize(true)) 
            throw new Exception("Theme repository initialization went wrong");

        var settingsRepository = new SettingsJSONRepository("settings.json");
        if (!settingsRepository.Initialize(true))
            throw new Exception("Setting repository initialization went wrong");

        var themes = themeRepository.Download();
        var settingsDic = settingsRepository.Download();
        var settings = new Settings(themes, settingsRepository);
        
        if (settingsDic is not null)
        {
            foreach (var entry in settingsDic)
            {
                settings.TrySet(entry.Key, entry.Value);
            }
        }

        var index = settings.Theme.Get().ToInt();
        return new GlobalApplicationPresenter(settings, index < 0 || index >= themes.Length ? null : themes[index]);
    }
}