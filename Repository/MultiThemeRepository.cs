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

    public int WritableCount() => _repositories[0].Count();

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
        _repositories[0].AddTheme(theme);
    }
}