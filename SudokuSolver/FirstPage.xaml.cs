using System.Windows;
using System.Windows.Controls;
using SudokuSolver.Pages;

namespace SudokuSolver;

public partial class FirstPage : Page
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
}