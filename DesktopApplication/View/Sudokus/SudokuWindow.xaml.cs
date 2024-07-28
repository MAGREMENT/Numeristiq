using System.Windows;
using System.Windows.Navigation;
using DesktopApplication.Presenter;
using DesktopApplication.View.Sudokus.Pages;

namespace DesktopApplication.View.Sudokus;

public partial class SudokuWindow
{
    private readonly ManagedPage[] _pages;

    private int _currentPage = -1;
    private bool _cancelNavigation = true;
    
    public SudokuWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        var presenter = GlobalApplicationPresenter.Instance.InitializeSudokuApplicationPresenter();
        _pages = new ManagedPage[]
        {
            new SolvePage(presenter), new PlayPage(presenter), new ManagePage(presenter), new GeneratePage(presenter)
        };

        SwapPage(0);
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
}