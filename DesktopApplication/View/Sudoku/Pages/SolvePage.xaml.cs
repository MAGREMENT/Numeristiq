using DesktopApplication.Controllers;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class SolvePage : ISolvePageView
{
    private readonly SolvePageController _controller;
    
    public SolvePage()
    {
        InitializeComponent();
        _controller = ControllerDistributor.Initialize(this);
    }
}