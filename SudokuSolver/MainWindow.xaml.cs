using System;
using System.Windows.Controls;
using SudokuSolver.Pages;

namespace SudokuSolver;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : IPageHandler
{
    private readonly Page[] _pages;
    
    public MainWindow()
    {
        InitializeComponent();
        _pages = new Page[]
        {
            new FirstPage(this), new MainSolverPage(this)
        };

        ShowPage(PagesName.First);
    }

    public void ShowPage(PagesName pageName)
    {
        Main.Content = _pages[(int)pageName];
    }
}
