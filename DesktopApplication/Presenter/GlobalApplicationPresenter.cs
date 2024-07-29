using System;
using DesktopApplication.Presenter.Kakuros;
using DesktopApplication.Presenter.Nonograms;
using DesktopApplication.Presenter.Sudokus;
using DesktopApplication.Presenter.Tectonics;
using DesktopApplication.Presenter.Themes;
using Model.Repositories;
using Repository;

namespace DesktopApplication.Presenter;

public class GlobalApplicationPresenter
{
    public const bool IsForProduction = false;
    
    private readonly Settings _settings;
    private readonly IGlobalApplicationView _view;
    private readonly ThemeManager _themeManager;

    private GlobalApplicationPresenter(IGlobalApplicationView view, MultiThemeRepository themeRepository,
        ISettingRepository settingsRepository)
    {
        _view = view;
        _themeManager = new ThemeManager(themeRepository);
        _settings = new Settings(_themeManager.Themes, settingsRepository);
        foreach (var entry in settingsRepository.GetSettings())
        {
            _settings.TrySet(entry.Key, entry.Value, false, false);
        }
        
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
        return new KakuroApplicationPresenter(_settings);
    }

    public NonogramApplicationPresenter InitializeNonogramApplicationPresenter()
    {
        return new NonogramApplicationPresenter();
    }

    public ThemePresenter InitializeThemePresenter(IThemeView view) => new(_themeManager, _settings, view);
    
    private void TrySetTheme()
    {
        var index = _settings.Theme;
        if(index < 0 || index >= _themeManager.Themes.Count) return;
        
        _view.SetTheme(_themeManager.Themes[index]);
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

    public static void InitializeInstance(IGlobalApplicationView view)
    {
        var themeRepository = new MultiThemeRepository(new JsonThemeRepository("themes.json",
                !IsForProduction, true),
            new HardCodedThemeRepository());
        var settingsRepository = new SettingsJsonRepository("settings.json", 
            !IsForProduction, true);

        _instance = new GlobalApplicationPresenter(view, themeRepository, settingsRepository);
    }

    #endregion
}