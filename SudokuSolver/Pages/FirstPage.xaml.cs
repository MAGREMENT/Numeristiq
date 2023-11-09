using System.Windows;
using System.Windows.Controls;

namespace SudokuSolver.Pages;

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