namespace DesktopApplication.Presenter;

public class WelcomePresenter
{ 
    public SettingsPresenter SettingsPresenter { get; }

    public WelcomePresenter(Settings settings)
    {
        SettingsPresenter = new SettingsPresenter(settings, SettingCollections.WelcomeWindow);
    }
}