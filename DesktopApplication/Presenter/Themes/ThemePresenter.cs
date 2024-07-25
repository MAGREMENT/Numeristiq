using Model.Core.Settings;
using Model.Utility;

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
            _view.SelectColor(name, _themeManager.Themes[_settings.Theme].GetColor(name));
        }
    }

    public void SetCurrentColor(RGB value)
    {
        if(_currentColor is null) return;

        var theme = _themeManager.Themes[_settings.Theme];
        theme.SetColor(_currentColor, value);
        //TODO update repo
        _settings.TrySet("Theme", new IntSettingValue(_settings.Theme));
        _view.SetColors(theme.AllColors(), _themeManager.IsEditable(_settings.Theme));
    }

    public void SetTheme(string name)
    {
        var index = _themeManager.IndexOf(name);

        if (index == -1 || index == _settings.Theme) return;

        _settings.TrySet("Theme", new IntSettingValue(index));
        UpdateThemeStuff();
    }

    public void EvaluateName(string name)
    {
        if (IsNameCorrect(name, out var error)) _view.ShowNameIsCorrect();
        else _view.ShowNameError(error);
    }

    public void SaveNewTheme(string name)
    {
        if (!IsNameCorrect(name, out _)) return;
        var theme = _themeManager.Themes[_settings.Theme].Copy(name);
        _themeManager.AddNewTheme(theme);
        
        _view.SetOtherThemes(_themeManager.EnumerateThemesAndState(_settings.Theme));
        _view.ShowNameError("Name is already used");
    }

    private bool IsNameCorrect(string name, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            error = "Name cannot be empty";
            return false;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            error = "Name cannot only be spaces";
            return false;
        }

        foreach (var c in name)
        {
            switch (c)
            {
                case '-' :
                case ' ' :
                    break;
                default:
                    if (!char.IsLetter(c) && !char.IsDigit(c))
                    {
                        error = $"Forbidden character : {c}";
                        return false;
                    }

                    break;
            }
        }

        if (_themeManager.IndexOf(name) != -1)
        {
            error = "Name is already used";
            return false;
        }

        return true;
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