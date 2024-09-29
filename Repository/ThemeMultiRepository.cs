using System.Windows;
using Model.Repositories;

namespace Repository;

public class ThemeMultiRepository : IThemeRepository
{
    private readonly List<IThemeRepository> _repositories = new();
    
    public int WritableStart { get; }

    public ThemeMultiRepository(IThemeRepository writable, params IThemeRepository[] others)
    {
        _repositories.AddRange(others);
        _repositories.Add(writable);

        for (int i = 0; i < _repositories.Count - 1; i++)
        {
            WritableStart += _repositories[i].Count();
        }
    }
    
    public IReadOnlyList<Theme> GetThemes()
    {
        List<Theme> result = new();
        foreach (var repo in _repositories)
        {
            result.AddRange(repo.GetThemes());
        }

        return result;
    }

    public int Count()
    {
        var total = 0;
        foreach (var repo in _repositories)
        {
            total += repo.Count();
        }

        return total;
    }

    public void AddTheme(Theme theme)
    {
        _repositories[^1].AddTheme(theme);
    }

    public void Remove(int index)
    {
        _repositories[^1].Remove(index - WritableStart);
    }

    public void ChangeTheme(int index, Theme newTheme)
    {
        _repositories[^1].ChangeTheme(index - WritableStart, newTheme);
    }

    public Theme? FindTheme(string name)
    {
        foreach (var repo in _repositories)
        {
            var t = repo.FindTheme(name);
            if (t is not null) return t;
        }

        return null;
    }

    public void ClearThemes()
    {
        _repositories[^1].ClearThemes();
    }
}