using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Sudokus.Pages;

namespace DesktopApplication.View.HelperWindows;

public partial class ChooseStepWindow : IStepChooserView
{
    private readonly StepChooserPresenter _presenter;
    
    public ChooseStepWindow(ChooseStepPresenterBuilder builder)
    {
        InitializeComponent();
        _presenter = builder.Build(this);
        
        _presenter.Initialize();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    public ISudokuSolverDrawer Drawer => (ISudokuSolverDrawer)Embedded.OptimizableContent!;
    
    public void SetTotalPage(int n)
    {
        PageSelector.Max = n;
    }

    public void SetCurrentPage(int n)
    {
        PageSelector.Current = n;
    }

    public void SetSelectionAvailability(bool yes)
    {
        SelectButton.IsEnabled = yes;
    }

    private void OnPageChange(int newPage)
    {
        _presenter.ChangePage(newPage);
    }

    private void OnSelection(object sender, RoutedEventArgs e)
    {
        _presenter.SelectCurrent();
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        Close();
    }

    public override StackPanel GetStepPanel()
    {
        return StepsPanel;
    }

    public override ScrollViewer GetStepViewer()
    {
        return StepViewer;
    }

    public override IStepManagingPresenter GetStepsPresenter()
    {
        return _presenter;
    }

    public override ISizeOptimizable GetExplanationDrawer()
    {
        return SolvePage.GetSudokuExplanationDrawer();
    }
}