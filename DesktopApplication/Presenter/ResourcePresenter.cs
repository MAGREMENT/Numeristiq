using Model.Repositories;

namespace DesktopApplication.Presenter;

public class ResourcePresenter
{
    private readonly Settings _settings;
    private readonly ThemeManager _themeManager;
    private readonly IResourceView _view;

    public ResourcePresenter(Settings settings, ThemeManager themeManager, IResourceView view)
    {
        _settings = settings;
        _themeManager = themeManager;
        _view = view;

        TrySetTheme();
        _settings.Theme.ValueChanged += _ => TrySetTheme();
    }

    private void TrySetTheme()
    {
        var index = _settings.Theme.Get().ToInt();
        if(index < 0 || index >= _themeManager.Themes.Count) return;
        
        _view.SetTheme(_themeManager.Themes[index]);
    }
}

public interface IResourceView
{
    void SetTheme(Theme theme);
}