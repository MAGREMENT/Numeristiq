using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.Presenter.Sudokus.Solve.ChooseStep;
using Model.Core;

namespace DesktopApplication.View.HelperWindows;

public partial class ChooseStepWindow : IChooseStepView
{
    private readonly ChooseStepPresenter _presenter;
    
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
    
    public void ClearCommits()
    {
        StepsPanel.Children.Clear();
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

    public void SelectStep(int index)
    {
        if (index < 0 || index >= StepsPanel.Children.Count || StepsPanel.Children[index] is not TextBlock tb) return;

        tb.FontWeight = FontWeights.SemiBold;
    }

    public void UnselectStep(int index)
    {
        if (index < 0 || index >= StepsPanel.Children.Count || StepsPanel.Children[index] is not TextBlock tb) return;

        tb.FontWeight = FontWeights.Normal;
    }

    public void EnableSelection(bool isEnabled)
    {
        SelectButton.IsEnabled = isEnabled;
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