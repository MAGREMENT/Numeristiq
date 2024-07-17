using System;
using System.Collections.Generic;
using DesktopApplication.Presenter.Kakuros;
using DesktopApplication.Presenter.Nonograms;
using DesktopApplication.Presenter.Sudokus;
using DesktopApplication.Presenter.Tectonics;
using Model.Repositories;
using Repository;

namespace DesktopApplication.Presenter;

public class GlobalApplicationPresenter
{
    public const bool IsForProduction = false;
    
    private readonly Settings _settings;
    private readonly IGlobalApplicationView _view;
    private readonly IReadOnlyList<Theme> _themes;

    private GlobalApplicationPresenter(IGlobalApplicationView view, Settings settings, IReadOnlyList<Theme> themes)
    {
        _view = view;
        _settings = settings;
        _themes = themes;
        
        TrySetTheme();
        _settings.ThemeSetting.ValueChanged += _ => TrySetTheme();
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
        if(index < 0 || index >= _themes.Count) return;
        
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
        var settingsRepository = new SettingsJsonRepository("settings.json", 
            !IsForProduction, true);

        var themes = themeRepository.GetThemes();
        var settingsDic = settingsRepository.GetSettings();
        var settings = new Settings(themes, settingsRepository);
        
        foreach (var entry in settingsDic)
        {
            settings.TrySet(entry.Key, entry.Value);
        }
        
        _instance = new GlobalApplicationPresenter(view, settings, themes);
        return _instance;
    }

    #endregion
}