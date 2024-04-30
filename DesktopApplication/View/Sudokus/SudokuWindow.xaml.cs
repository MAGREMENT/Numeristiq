using System.Windows;
using System.Windows.Navigation;
using DesktopApplication.Presenter;
using DesktopApplication.View.Sudokus.Pages;

namespace DesktopApplication.View.Sudokus;

public partial class SudokuWindow
{
    private readonly SudokuPage[] _pages;

    private int _currentPage = -1;
    private bool _cancelNavigation = true;
    private readonly bool _initialized;
    
    public SudokuWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        var presenter = GlobalApplicationPresenter.Instance.InitializeSudokuApplicationPresenter();
        presenter.InitializeApplication();
        _pages = new SudokuPage[]
        {
            new SolvePage(presenter), new PlayPage(presenter), new ManagePage(presenter), new GeneratePage(presenter)
        };

        SwapPage(0);

        _initialized = true;
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void SwapPage(int number)
    {
        _cancelNavigation = false;
        if(_currentPage != -1) _pages[_currentPage].OnClose();
        
        _currentPage = number;
        var newOne = _pages[_currentPage];

        TitleBar.InsideContent = newOne.TitleBarContent();
        newOne.OnShow();
        Frame.Content = newOne;

        _cancelNavigation = true;
    }

    private void CancelNavigation(object sender, NavigatingCancelEventArgs e)
    {
        if(_cancelNavigation) e.Cancel = true;
    }

    private void UpdatePageSize(object sender, SizeChangedEventArgs e)
    {
        if (!_initialized) return;
        
        Frame.Width = ActualWidth;
        Frame.Height = ActualHeight - 90;
        foreach (var page in _pages)
        {
            page.UpdateSize(Frame.Width, Frame.Height);   
        }
    }
}