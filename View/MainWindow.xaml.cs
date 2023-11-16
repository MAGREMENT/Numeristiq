using System.Windows.Controls;
using View.Pages;
using View.Pages.Solver;

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
        _pages = new Page[]
        {
            new FirstPage(this), new SolverPage(this)
        };

        ShowPage(PagesName.First);
    }

    public void ShowPage(PagesName pageName)
    {
        Main.Content = _pages[(int)pageName];
    }
}
