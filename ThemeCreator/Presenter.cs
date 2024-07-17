using System.Collections.Generic;
using Model.Repositories;
using Model.Utility;
using Repository;

namespace ThemeCreator;

public class Presenter
{
    private readonly IMainView _view;
    private readonly IThemeRepository _repository;
    private readonly List<Theme> _themes;
    private readonly int _current;

    public Presenter(IMainView view)
    {
        _view = view;
        _repository = new HardCodedThemeRepository();
        _themes = new List<Theme>(_repository.GetThemes());
        
        _view.SetColors(_themes[_current].AllColors());
    }
}

public interface IMainView
{
    void SetColors(IEnumerable<(string, RGB)> colors);
}