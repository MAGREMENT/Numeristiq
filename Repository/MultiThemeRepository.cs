using Model.Repositories;

namespace Repository;

public class MultiThemeRepository : IThemeRepository
{
    private readonly List<IThemeRepository> _repositories = new();

    public MultiThemeRepository(IThemeRepository writable, params IThemeRepository[] others)
    {
        _repositories.Add(writable);
        _repositories.AddRange(others);
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

    public List<(Theme, bool)> GetThemesAndState()
    {
        var result = new List<(Theme, bool)>();

        for (int i = 0; i < _repositories.Count; i++)
        {
            var themes = _repositories[i].GetThemes();
            foreach (var t in themes)
            {
                result.Add((t, i == 0));
            }
        }
        
        return result;
    }

    public void AddTheme(Theme theme)
    {
        _repositories[0].AddTheme(theme);
    }
}