using System.ComponentModel;
using System.Windows;
using Global;
using Global.Enums;
using Presenter;
using Presenter.StepChooser;
using Presenter.Translators;
using View.Themes;

namespace View.HelperWindows.StepChooser;

public partial class StepChooserWindow : IStepChooserView
{
    private StepChooserPresenter? _presenter;
    
    public StepChooserWindow()
    {
        InitializeComponent();
        
        Closing += OnClose;
        CommitList.CommitSelected += OnCommitSelect;
        CommitInfo.HighlightShifted += OnShiftHighlight;
    }

    public void SetPresenter(StepChooserPresenterBuilder builder)
    {
        _presenter = builder.Build(this);
        _presenter.Bind();
    }

    public void PutCursorOn(Cell cell)
    {
        Grid.PutCursorOn(cell);
    }

    public void ClearCursor()
    {
        Grid.ClearCursor();
    }

    public void ClearNumbers()
    {
        Grid.ClearNumber();
    }

    public void ShowSolution(int row, int col, int number, CellColor color)
    {
        Grid.SetCellTo(row, col, number, color);
    }

    public void ShowPossibilities(int row, int col, int[] possibilities, CellColor color)
    {
        Grid.SetCellTo(row, col, possibilities, color);
    }

    public void ShowCommits(ViewCommit[] commits)
    {
        CommitList.Show(commits);
    }

    public void ShowCommitInformation(ViewCommitInformation commit)
    {
        CommitInfo.Show(commit);
    }

    public void StopShowingCommitInformation()
    {
        CommitInfo.StopShow();
    }

    public void ShowSelection(int n)
    {
        CommitList.ShowSelection(n);
    }

    public void UnShowSelection(int n)
    {
        CommitList.UnShowSelection(n);
    }

    public void AllowChoosing(bool yes)
    {
        ChooseButton.IsEnabled = yes;
    }

    public void Refresh()
    {
        Grid.Refresh();
    }

    public void ClearDrawings()
    {
        Grid.ClearDrawings();
    }

    public void FillPossibility(int row, int col, int possibility, ChangeColoration coloration)
    {
        Grid.FillPossibility(row, col, possibility, coloration);
    }

    public void FillCell(int row, int col, ChangeColoration coloration)
    {
        Grid.FillCell(row, col, coloration);
    }

    public void EncirclePossibility(int row, int col, int possibility)
    {
        Grid.EncirclePossibility(row, col, possibility);
    }

    public void EncircleCell(int row, int col)
    {
        Grid.EncircleCell(row, col);
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        ChangeColoration coloration)
    {
        Grid.EncircleRectangle(rowFrom, colFrom, possibilityFrom, rowTo, colTo, possibilityTo, coloration);
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, ChangeColoration coloration)
    {
        Grid.EncircleRectangle(rowFrom, colFrom, rowTo, colTo, coloration);
    }

    public void EncircleCellPatch(Cell[] cells, ChangeColoration coloration)
    {
        Grid.EncircleCellPatch(cells, coloration);
    }

    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority)
    {
        Grid.CreateLink(rowFrom, colFrom, possibilityFrom, rowTo, colTo, possibilityTo, strength, priority);
    }

    private void Choose(object sender, RoutedEventArgs e)
    {
        _presenter?.SelectCurrentCommit();
        Close();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void OnClose(object? sender, CancelEventArgs args)
    {
        _presenter?.Closed();
    }

    private void OnCommitSelect(int n)
    {
        _presenter?.SelectCommit(n);
    }

    private void OnShiftHighlight(int n)
    {
        _presenter?.ShiftHighlighting(n);
    }
}