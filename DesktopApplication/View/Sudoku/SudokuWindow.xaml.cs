using System.Windows;
using DesktopApplication.Presenter;
using DesktopApplication.View.Sudoku.Pages;

namespace DesktopApplication.View.Sudoku;

public partial class SudokuWindow
{
    private readonly SudokuPage[] _pages;

    private int _currentPage = -1;
    
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
        if(_currentPage != -1) _pages[_currentPage].OnClose();
        
        _currentPage = number;
        var newOne = _pages[_currentPage];

        TitleBar.InsideContent = newOne.TitleBarContent();
        newOne.OnShow();
        Frame.Content = newOne;
    }
}