using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;
using DesktopApplication.View.Controls;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

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
    
    public void ClearSteps()
    {
        StepsPanel.Children.Clear();
    }

    public void AddStep(int index, BuiltChangeCommit<NumericChange, ISudokuHighlighter> commit)
    {
        var control = new StepControl(index + 1, commit);
        control.OpenRequested += i => _presenter.ShowStep(i - 1);
        StepsPanel.Children.Add(control);
    }

    public void AddCommit(Strategy maker, int index)
    {
        var tb = new TextBlock
        {
            Text = maker.Name,
            Style = (Style)FindResource("SearchResult")
        };
        tb.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(maker.Difficulty));
        
        tb.MouseLeftButtonDown += (_, _) => _presenter.ShowStep(index);
        
        StepsPanel.Children.Add(tb);
    }

    public void SetTotalPage(int n)
    {
        PageSelector.Max = n;
    }

    public void SetCurrentPage(int n)
    {
        PageSelector.Current = n;
    }

    public void OpenStep(int index)
    {
        ((StepControl)StepsPanel.Children[index]).Open();
    }

    public void CloseStep(int index)
    {
        ((StepControl)StepsPanel.Children[index]).Close();
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
}