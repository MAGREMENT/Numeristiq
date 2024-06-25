using System;
using System.Collections.Generic;
using DesktopApplication.Presenter.Kakuros;
using DesktopApplication.Presenter.Nonograms;
using DesktopApplication.Presenter.Sudokus;
using DesktopApplication.Presenter.Tectonics;
using Model;
using Model.Core.Settings;
using Repository;

namespace DesktopApplication.Presenter;

public class GlobalApplicationPresenter
{
    private const bool IsForProduction = false;
    
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

    public KakuroApplicationPresenter InitializeKakuroApplicationPresenter()
    {
        return new KakuroApplicationPresenter();
    }

    public NonogramApplicationPresenter InitializeNonogramApplicationPresenter()
    {
        return new NonogramApplicationPresenter();
    }
    
    private void TrySetTheme()
    {
        var index = _settings.Theme;
        if(index < 0 || index >= _themes.Length) return;
        
        _view.SetTheme(_themes[index]);
    }

    #region Instance

    private static GlobalApplicationPresenter? _instance;
    private static PathInstantiator? _pathInstantiator;

    public static GlobalApplicationPresenter Instance
    {
        get
        {
            if (_instance is null) throw new Exception("Not initialized");
            return _instance;
        }
    }

    public static PathInstantiator PathInstantiator
    {
        get
        {
            _pathInstantiator ??= new PathInstantiator(!IsForProduction, true);
            return _pathInstantiator;
        }
    }

    public static GlobalApplicationPresenter InitializeInstance(IGlobalApplicationView view)
    {
        var themeRepository = new HardCodedThemeRepository();
        
        var settingsRepository = new JSONRepository<Dictionary<string, SettingValue>>(PathInstantiator.Instantiate("settings.json"));

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