using System.Windows.Controls;
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
    private readonly Page[] _pages;
    
    public MainWindow()
    {
        InitializeComponent();

        var factory = new PresenterFactory();
        
        _pages = new Page[]
        {
            new FirstPage(this), new SolverPage(this, factory), new StrategyManagerPage(this, factory)
        };

        ShowPage(PagesName.First);
    }

    public void ShowPage(PagesName pageName)
    {
        Main.Content = _pages[(int)pageName];
    }
}
