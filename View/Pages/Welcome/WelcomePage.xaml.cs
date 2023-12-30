using System.Windows;
using System.Windows.Media;
using View.Themes;

namespace View.Pages.Welcome;

public partial class WelcomePage
{
    private readonly IPageHandler _pageHandler;
    
    public WelcomePage(IPageHandler pageHandler)
    {
        InitializeComponent();
        _pageHandler = pageHandler;
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
    }
}