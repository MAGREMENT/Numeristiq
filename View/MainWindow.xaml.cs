using Presenter;
using View.Pages;
using View.Pages.Solver;
using View.Pages.StrategyManager;

namespace View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : IPageHandler
{
    private readonly HandledPage[] _pages;
    private HandledPage? _currentlyShown;
    
    public MainWindow()
    {
        InitializeComponent();

        var factory = new PresenterFactory();
        
        _pages = new HandledPage[]
        {
            new FirstPage(this), new SolverPage(this, factory), new StrategyManagerPage(this, factory)
        };

        ShowPage(PagesName.First);
    }

    public void ShowPage(PagesName pageName)
    {
        if(_currentlyShown is not null) _currentlyShown.OnQuit();

        var page = _pages[(int)pageName];
        Main.Content = page;
        _currentlyShown = page;
        
        page.OnShow();
    }
}
