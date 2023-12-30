using System.Windows.Forms;
using System.Windows.Interop;
using Presenter;
using Presenter.Translators;
using View.Pages;
using View.Pages.Player;
using View.Pages.Solver;
using View.Pages.StrategyManager;
using View.Pages.Welcome;
using View.Themes;

namespace View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : IPageHandler, IViewManager
{
    private readonly HandledPage[] _pages;
    private HandledPage? _currentlyShown;

    private bool _navigationAllowed;
    
    public MainWindow()
    {
        InitializeComponent();

        var presenter = ApplicationPresenter.Initialize(this);
        
        _pages = new HandledPage[]
        {
            new WelcomePage(this), new SolverPage(this, presenter),
            new PlayerPage(this, presenter), new StrategyManagerPage(this, presenter)
        };

        Main.NavigationService.Navigating += (_, args) =>
        {
            if (!_navigationAllowed) args.Cancel = true;
        };

        //SizeChanged += (_, _) => Center(); TODO fix
        presenter.ViewInitializationFinished();
        ShowPage(PagesName.First);
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
        foreach (var page in _pages)
        {
            page.ApplyTheme(theme);
        }
    }

    private void Center()
    {
        var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
        
        Left = screen.WorkingArea.Left + (screen.WorkingArea.Width - ActualWidth) / 2;
        Top = screen.WorkingArea.Top + (screen.WorkingArea.Height - ActualHeight) / 2;
    }
}
