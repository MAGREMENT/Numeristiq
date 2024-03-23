using System;
using System.Collections.Generic;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Tectonic;
using Model;
using Model.Helpers.Settings;
using Repository;

namespace DesktopApplication.Presenter;

public class GlobalApplicationPresenter
{
    private readonly Settings _settings;
    private readonly IGlobalApplicationView _view;
    private readonly Theme[] _themes;

    private GlobalApplicationPresenter(IGlobalApplicationView view, Settings settings, Theme[] themes)
    {
        _view = view;
        _settings = settings;
        _themes = themes;
        
        TrySetTheme();
        _settings.AddEvent(SpecificSettings.Theme, _ => TrySetTheme());
    }

    public WelcomePresenter InitializeWelcomePresenter()
    {
        return new WelcomePresenter(_settings);
    }

    public SudokuApplicationPresenter InitializeSudokuApplicationPresenter()
    {
        return new SudokuApplicationPresenter(_settings);
    }

    public TectonicApplicationPresenter InitializeTectonicApplicationPresenter()
    {
        return new TectonicApplicationPresenter(_settings);
    }
    
    private void TrySetTheme()
    {
        var index = _settings.Theme;
        if(index < 0 || index >= _themes.Length) return;
        
        _view.SetTheme(_themes[index]);
    }

    #region Instance

    private static GlobalApplicationPresenter? _instance;

    public static GlobalApplicationPresenter Instance
    {
        get
        {
            if (_instance is null) throw new Exception("Not initialized");
            return _instance;
        }
    }

    public static GlobalApplicationPresenter InitializeInstance(IGlobalApplicationView view)
    {
        var themeRepository = new HardCodedThemeRepository();
        if (!themeRepository.Initialize(true)) 
            throw new Exception("Theme repository initialization went wrong");

        var settingsRepository = new JSONRepository<Dictionary<string, SettingValue>>("settings.json");
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
        
        _instance = new GlobalApplicationPresenter(view, settings, themes);
        return _instance;
    }

    #endregion
}