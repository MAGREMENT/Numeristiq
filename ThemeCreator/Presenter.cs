using System.Collections.Generic;
using Model.Repositories;
using Model.Utility;
using Repository;

namespace ThemeCreator;

public class Presenter
{
    private const bool IsForProduction = false;
    
    private readonly IMainView _view;
    private readonly MultiThemeRepository _repository;
    private readonly List<(Theme, bool)> _themes;
    private int _currentTheme;
    private string? _currentColor;

    public Presenter(IMainView view)
    {
        _view = view;
        _repository = new MultiThemeRepository(new JsonThemeRepository("themes.json",
            !IsForProduction, true), new HardCodedThemeRepository());
        _themes = _repository.GetThemesAndState();

        _view.SetCurrentTheme(_themes[_currentTheme].Item1);
        _view.SetOtherThemes(GetOtherThemes());
        _view.SetColors(_themes[_currentTheme].Item1.AllColors());
    }

    public void SelectColor(string name)
    {
        if (name.Equals(_currentColor))
        {
            _currentColor = null;
            _view.UnselectColor();
        }
        else
        {
            _currentColor = name;
            _view.SelectColor(name);
        }
    }

    private IEnumerable<Theme> GetOtherThemes()
    {
        for(int i = 0; i < _themes.Count; i++)
        {
            if (i != _currentTheme) yield return _themes[i].Item1;
        }
    }
}

public interface IMainView
{
    void SetCurrentTheme(Theme theme);
    void SetOtherThemes(IEnumerable<Theme> themes);
    void SetColors(IEnumerable<(string, RGB)> colors);
    void SelectColor(string name);
    void UnselectColor();
}