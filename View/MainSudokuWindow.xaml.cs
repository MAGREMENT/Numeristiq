using Presenter.Sudoku;
using Presenter.Sudoku.Translators;
using View.Pages;
using View.Pages.Player;
using View.Pages.Solver;
using View.Pages.StrategyManager;
using View.Pages.Welcome;
using View.Themes;
using Application = System.Windows.Application;

namespace View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainSudokuWindow : IPageHandler, IViewManager
{
    private readonly HandledPage[] _pages;
    private HandledPage? _currentlyShown;

    private bool _navigationAllowed;
    
    public MainSudokuWindow()
    {
        InitializeComponent();

        var presenter = ApplicationPresenter.Initialize(this);
        
        _pages = new HandledPage[]
        {
            new WelcomePage(this, presenter.GlobalSettings), new SolverPage(this, presenter),
            new PlayerPage(this, presenter), new StrategyManagerPage(this, presenter)
        };

        Main.NavigationService.Navigating += (_, args) =>
        {
            if (!_navigationAllowed) args.Cancel = true;
        };
        
        presenter.ViewInitializationFinished();
        ShowPage(PagesName.First);

        Closed += (_, _) => presenter.Close();
    }

    public void ShowPage(PagesName pageName)
    {
        _navigationAllowed = true;
        _currentlyShown?.OnQuit();

        var page = _pages[(int)pageName];
        Main.Content = page;
        _currentlyShown = page;
        
        _currentlyShown.OnShow();
        _navigationAllowed = false;
    }

    public void ApplyTheme(ViewTheme dao)
    {
        var theme = Theme.From(dao);
        ((App)Application.Current).ChangeTheme(theme);
    }
}
