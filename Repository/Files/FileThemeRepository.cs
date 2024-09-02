using Model.Repositories;
using Model.Utility;

namespace Repository.Files;

public class FileThemeRepository : FileRepository<List<ThemeDAO>>, IThemeRepository
{
    private List<ThemeDAO>? _buffer;
    
    public FileThemeRepository(string fileName, bool searchParentDirectories, bool createIfNotFound,
        IFileType<List<ThemeDAO>> type) : base(fileName, searchParentDirectories, createIfNotFound, type)
    {
    }
    
    public IReadOnlyList<Theme> GetThemes()
    {
        _buffer ??= Download();
        return _buffer is null ? Array.Empty<Theme>() : DAOManager.To(_buffer);
    }

    public int Count()
    {
        _buffer ??= Download();
        return _buffer?.Count ?? 0;
    }

    public void AddTheme(Theme theme)
    {
        _buffer ??= Download() ?? new List<ThemeDAO>();

        _buffer.Add(DAOManager.To(theme));
        Upload(_buffer);
    }

    public void ChangeTheme(int index, Theme newTheme) //TODO FIX !!!!!!
    {
        _buffer ??= Download();
        if (_buffer is null || index < 0 || index >= _buffer.Count) return;

        _buffer[index] = DAOManager.To(newTheme);
        Upload(_buffer);
    }

    public Theme? FindTheme(string name)
    {
        _buffer ??= Download();
        if (_buffer is null) return null;
        foreach (var theme in _buffer)
        {
            if (theme.Name.Equals(name)) return DAOManager.To(theme);
        }

        return null;
    }

    public void ClearThemes()
    {
        _buffer = null;
        Upload(new List<ThemeDAO>());
    }

    
}