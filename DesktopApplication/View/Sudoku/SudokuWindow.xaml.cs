using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.View.Sudoku.Pages;

namespace DesktopApplication.View.Sudoku;

public partial class SudokuWindow
{
    private readonly SudokuApplicationPresenter _presenter;
    private readonly Page[] _pages;

    private int _currentPage;
    
    public SudokuWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        _presenter = GlobalApplicationPresenter.Instance.InitializeSudokuApplicationPresenter();
        _presenter.InitializeApplication();
        _pages = new Page[] { new SolvePage(_presenter), new PlayPage(_presenter), new ManagePage(_presenter), new GeneratePage(_presenter) };

        Frame.Content = _pages[_currentPage];
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
        _currentPage = number;
        Frame.Content = _pages[_currentPage];
    }
}