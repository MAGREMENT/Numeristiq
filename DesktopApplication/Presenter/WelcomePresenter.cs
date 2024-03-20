namespace DesktopApplication.Presenter;

public class WelcomePresenter
{
    private readonly Settings _settings;
    
    public SettingsPresenter SettingsPresenter { get; }

    public WelcomePresenter(Settings settings)
    {
        _settings = settings;
        SettingsPresenter = new SettingsPresenter(settings, SettingCollections.WelcomeWindow);
    }
}