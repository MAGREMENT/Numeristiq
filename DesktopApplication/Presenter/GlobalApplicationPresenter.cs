using Model;
using Repository;

namespace DesktopApplication.Presenter;

public class GlobalApplicationPresenter
{
    private readonly IRepository<ChosenTheme> _themeRepository = new HardCodedThemeRepository();
    
    public Theme? Theme { get; private set; }

    public void Initialize()
    {
        if (_themeRepository.Initialize(true))
        {
            var download = _themeRepository.Download();
            if (download is null || download.Themes.Length == 0 || download.ChosenOne < 0 
                || download.ChosenOne >= download.Themes.Length) return;

            Theme = download.Themes[download.ChosenOne];
        }
    }
}