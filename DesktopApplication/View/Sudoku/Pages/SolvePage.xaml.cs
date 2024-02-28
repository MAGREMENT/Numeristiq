using DesktopApplication.Controllers;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class SolvePage : ISolvePageView
{
    public SolvePage()
    {
        InitializeComponent();
        ControllerDistributor.Initialize(this);
    }
}