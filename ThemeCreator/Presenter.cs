using System.Collections.Generic;
using Model.Repositories;
using Model.Utility;
using Repository;

namespace ThemeCreator;

public class Presenter
{
    private const bool IsForProduction = false;
    
    private readonly IMainView _view;
    private readonly IThemeRepository _repository;
    private readonly List<Theme> _themes;
    private Theme _current;

    public Presenter(IMainView view)
    {
        _view = view;
        _repository = new MultiThemeRepository(new JsonThemeRepository("themes.json",
            !IsForProduction, true), new HardCodedThemeRepository());
        _themes = new List<Theme>(_repository.GetThemes());

        _current = _themes[0].Copy();

        _view.SetCurrentTheme(_current);
        _view.SetOtherThemes(GetOtherThemes());
        _view.SetColors(_current.AllColors());
    }

    private IEnumerable<Theme> GetOtherThemes()
    {
        foreach (var t in _themes)
        {
            if(!t.Name.Equals(_current.Name)) yield return t;
        }
    }
}

public interface IMainView
{
    void SetCurrentTheme(Theme theme);
    void SetOtherThemes(IEnumerable<Theme> themes);
    void SetColors(IEnumerable<(string, RGB)> colors);
}