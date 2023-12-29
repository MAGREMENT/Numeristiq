using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using View.Themes;

namespace View.Pages;

public partial class FirstPage
{
    private readonly IPageHandler _pageHandler;
    
    public FirstPage(IPageHandler pageHandler)
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

    public override void Apply(Theme theme)
    {
        Background = new SolidColorBrush(theme.Background1);
    }
}