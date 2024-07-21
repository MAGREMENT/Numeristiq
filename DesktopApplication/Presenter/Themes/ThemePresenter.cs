using Model.Core.Settings;

namespace DesktopApplication.Presenter.Themes;

public class ThemePresenter
{
    private readonly IThemeView _view;
    private readonly ThemeManager _themeManager;
    private readonly Settings _settings;
    private string? _currentColor;

    public ThemePresenter(ThemeManager themeManager, Settings setting, IThemeView view)
    {
        _view = view;
        _themeManager = themeManager;
        _settings = setting;

        UpdateThemeStuff();
    }

    public void SelectColor(string name)
    {
        if (name.Equals(_currentColor))
        {
            UnselectCurrentColor();
        }
        else
        {
            _currentColor = name;
            _view.SelectColor(name);
        }
    }

    public void SetTheme(string name)
    {
        var index = _themeManager.IndexOf(name);

        if (index == -1 || index == _settings.Theme) return;

        _settings.TrySet("Theme", new IntSettingValue(index));
        UpdateThemeStuff();
    }

    private void UpdateThemeStuff()
    {
        var current = _themeManager.Themes[_settings.Theme];

        _view.SetCurrentTheme(current.Name);
        _view.SetOtherThemes(_themeManager.EnumerateThemesAndState(_settings.Theme));
        _view.SetColors(current.AllColors(), _themeManager.IsEditable(_settings.Theme));

        UnselectCurrentColor();
    }

    private void UnselectCurrentColor()
    {
        _currentColor = null;
        _view.UnselectColor();
    }
}