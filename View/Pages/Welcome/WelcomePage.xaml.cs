using System.Windows;
using Presenter;
using View.HelperWindows.Settings;
using View.Themes;

namespace View.Pages.Welcome;

public partial class WelcomePage
{
    private readonly IPageHandler _pageHandler;
    private readonly IGlobalSettings _settings;
    
    public WelcomePage(IPageHandler pageHandler, IGlobalSettings settings)
    {
        InitializeComponent();
        _pageHandler = pageHandler;
        _settings = settings;
    }

    private void GoToSolver(object sender, RoutedEventArgs e)
    {
        _pageHandler.ShowPage(PagesName.Solver);
    }

    private void GoToStrategyManager(object sender, RoutedEventArgs e)
    {
        _pageHandler.ShowPage(PagesName.StrategyManager);
    }
    
    private void GoToPlayer(object sender, RoutedEventArgs e)
    {
        _pageHandler.ShowPage(PagesName.Player);
    }

    public override void OnShow()
    {
        
    }

    public override void OnQuit()
    {
        
    }

    public override void ApplyTheme(Theme theme)
    {
        Background = theme.Background1;
        foreach (var b in Buttons.Children)
        {
            (b as IThemeable)?.ApplyTheme(theme);
        }

        SettingsButton.ApplyTheme(theme);
        Logo.ApplyTheme(theme);
    }

    private void ShowSettings(object? sender, RoutedEventArgs args)
    {
        var window = SettingsWindow.From(_settings);
        window.Show();
    }
}