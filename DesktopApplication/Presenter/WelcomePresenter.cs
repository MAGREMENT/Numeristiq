namespace DesktopApplication.Presenter;

public class WelcomePresenter
{
    private readonly Settings _settings;

    public SettingsSpan Settings => _settings.WelcomeWindowSettings;

    public WelcomePresenter(Settings settings)
    {
        _settings = settings;
    }
}