using System.Collections.Generic;
using Model.Repositories;
using Repository;

namespace DesktopApplication.Presenter;

public class ThemeManager
{
    private readonly ThemeMultiRepository _repository;
    private readonly List<Theme> _themes;

    public IReadOnlyList<Theme> Themes => _themes;

    public ThemeManager(ThemeMultiRepository repository)
    {
        _repository = repository;
        _themes = new List<Theme>(_repository.GetThemes());
    }

    public IEnumerable<(Theme, bool)> EnumerateThemesAndState(int except)
    {
        for (int i = 0; i < _themes.Count; i++)
        { 
            if(i != except) yield return (_themes[i], IsEditable(i));
        }
    }

    public bool IsEditable(int index)
    {
        return index >= _repository.WritableStart;
    }

    public int IndexOf(string name)
    {
        for (int i = 0; i < _themes.Count; i++)
        {
            if (_themes[i].Name.Equals(name))
            {
                return i;
            }
        }

        return -1;
    }

    public void AddNewTheme(Theme theme)
    {
        _repository.AddTheme(theme);
        _themes.Add(theme);
    }

    public int Remove(int index)
    {
        _themes.RemoveAt(index);
        _repository.Remove(index);
        
        return index == 0 ? index : index - 1;
    }

    public void UpdateTheme(int index)
    {
        _repository.ChangeTheme(index, _themes[index]);
    }
}