using System.Windows;
using Presenter;
using Presenter.Sudoku;
using View.HelperWindows.Settings;
using View.Themes;

namespace View.Pages.Welcome;

public partial class WelcomePage
{
    private readonly IPageHandler _pageHandler;

    private readonly SettingsWindow _settingsWindow;
    
    public WelcomePage(IPageHandler pageHandler, IGlobalSettings settings)
    {
        InitializeComponent();
        _pageHandler = pageHandler;

        _settingsWindow = SettingsWindow.From(settings);
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
    
    private void ShowSettings(object? sender, RoutedEventArgs args)
    {
        _settingsWindow.Refresh();
        _settingsWindow.Show();
    }
}