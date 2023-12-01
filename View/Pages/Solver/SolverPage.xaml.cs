using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Global;
using Global.Enums;
using Presenter;
using Presenter.Solver;
using Presenter.Translators;
using View.HelperWindows.Print;
using View.HelperWindows.Settings;
using View.Utility;

namespace View.Pages.Solver;

public partial class SolverPage : ISolverView
{
    private bool _createNewSudoku = true;

    private readonly SolverPresenter _presenter;
    private readonly IPageHandler _pageHandler;

    public SolverPage(IPageHandler pageHandler, PresenterFactory factory)
    {
        InitializeComponent();

        _presenter = factory.Create(this);
        _presenter.Bind();
        _pageHandler = pageHandler;

        LogList.ChangeStateShown(_presenter.Settings.StateShown);

        Solver.CellSelected += _presenter.SelectCell;
        Solver.CellUnselected += _presenter.UnSelectCell;
        Solver.CurrentCellChangeAsked += _presenter.ChangeCurrentCell;
        Solver.RemoveSolutionFromCurrentCellAsked += _presenter.RemoveCurrentCell;
        LogList.LogSelected += _presenter.SelectLog;
        LogList.ShowStartStateAsked += _presenter.ShowStartState;
        LogList.ShowCurrentStateAsked += _presenter.ShowCurrentState;
        LogList.StateShownChanged += ss => _presenter.Settings.StateShown = ss;
        LogList.LogHighlightShifted += _presenter.ShiftLogHighlight;
        StrategyList.StrategyUsed += _presenter.UseStrategy;
    }
    
    //ISolverView-------------------------------------------------------------------------------------------------------

    public void DisableActions()
    {
        Dispatcher.Invoke(() =>
        {
            SolveButton.IsEnabled = false;
            ClearButton.IsEnabled = false; 
        });
    }

    public void EnableActions()
    {
        Dispatcher.Invoke(() =>
        {
            SolveButton.IsEnabled = true;
            ClearButton.IsEnabled = true;
        });
    }

    public void SetCellTo(int row, int col, int number)
    { 
        
        Solver.Dispatcher.Invoke(() => Solver.SetCellTo(row, col, number));
    }

    public void SetCellTo(int row, int col, int[] possibilities)
    {
        Solver.Dispatcher.Invoke(() => Solver.SetCellTo(row, col, possibilities));
    }

    public void UpdateGivens(HashSet<Cell> givens, CellColor solvingColor, CellColor givenColor)
    {
        Solver.Dispatcher.Invoke(() => Solver.UpdateGivens(givens, solvingColor, givenColor));
    }

    public void SetTranslation(string translation)
    {
        _createNewSudoku = false;
        SudokuStringBox.Dispatcher.Invoke(() => SudokuStringBox.Text = translation);
        _createNewSudoku = true;
    }

    public void FocusLog(int number)
    {
        LogList.Dispatcher.Invoke(() => LogList.FocusLog(number));
    }

    public void UnFocusLog()
    {
        LogList.Dispatcher.Invoke(() => LogList.UnFocusLog());
    }

    public void ShowExplanation(string explanation)
    { 
        ExplanationBox.Dispatcher.Invoke(() => ExplanationBox.Text = explanation);
    }

    public void SetLogs(IReadOnlyList<ViewLog> logs)
    {
        LogList.Dispatcher.Invoke(() => LogList.SetLogs(logs));
    }

    public void UpdateFocusedLog(ViewLog log)
    {
        LogList.UpdateFocusedLog(log);
    }

    public void InitializeStrategies(IReadOnlyList<ViewStrategy> strategies)
    {
        StrategyList.Dispatcher.Invoke(() => StrategyList.InitializeStrategies(strategies));
    }

    public void UpdateStrategies(IReadOnlyList<ViewStrategy> strategies)
    { 
        StrategyList.Dispatcher.Invoke(() => StrategyList.UpdateStrategies(strategies));
    }

    public void LightUpStrategy(int number)
    {
        StrategyList.Dispatcher.Invoke(() => StrategyList.LightUpStrategy(number));
    }

    public void PutCursorOn(Cell cell)
    {
        Solver.Dispatcher.Invoke(() => Solver.PutCursorOn(cell));
    }

    public void ClearCursor()
    {
        Solver.Dispatcher.Invoke(Solver.ClearCursor);
    }

    public void UpdateBackground()
    {
        Solver.Dispatcher.Invoke(Solver.UpdateBackground);
    }

    public void ToClipboard(string s)
    {
        Clipboard.SetText(s);
    }

    public void ShowFullScan(string s)
    {
        var printWindow = new PrintWindow("Full Scan", s);
        printWindow.Show();
    }

    public void ShowAllStrategies(string s)
    {
        var printWindow = new PrintWindow("All Strategies", s);
        printWindow.Show();
    }

    public void ClearDrawings()
    {
        Solver.Dispatcher.Invoke(Solver.ClearBackground);
    }

    public void FillPossibility(int row, int col, int possibility, ChangeColoration coloration)
    {
        Solver.Dispatcher.Invoke(() => Solver.FillPossibility(row, col, possibility, coloration));
    }

    public void FillCell(int row, int col, ChangeColoration coloration)
    {
        Solver.Dispatcher.Invoke(() => Solver.FillCell(row, col, coloration));
    }

    public void EncirclePossibility(int row, int col, int possibility)
    {
        Solver.Dispatcher.Invoke(() => Solver.EncirclePossibility(row, col, possibility));
    }

    public void EncircleCell(int row, int col)
    {
        Solver.Dispatcher.Invoke(() => Solver.EncircleCell(row, col));
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        ChangeColoration coloration)
    {
        Solver.Dispatcher.Invoke(() => Solver.EncircleRectangle(rowFrom, colFrom, possibilityFrom, rowTo, colTo,
            possibilityTo, coloration));
    }

    public void EncircleCellPatch(Cell[] cells, ChangeColoration coloration)
    {
        Solver.Dispatcher.Invoke(() => Solver.EncircleCellPatch(cells, coloration));
    }

    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority)
    {
        Solver.Dispatcher.Invoke(() => Solver.CreateLink(rowFrom, colFrom, possibilityFrom, rowTo, colTo,
            possibilityTo, strength, priority));
    }

    //EventHandling-----------------------------------------------------------------------------------------------------

    private void NewSudoku(object sender, TextChangedEventArgs e)
    {
        if (_createNewSudoku) _presenter.NewSudokuFromString(SudokuStringBox.Text);
    }

    private void SolveSudoku(object sender, RoutedEventArgs e)
    {
        _presenter.Solve();
    }

    private void ClearSudoku(object sender, RoutedEventArgs e)
    {
        _presenter.ClearSudoku();
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        _pageHandler.ShowPage(PagesName.First);
    }

    private void ShowSettingsWindow(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SolverSettingsWindow(_presenter.Settings);
        settingsWindow.Show();
    }

    private void Copy(object sender, RoutedEventArgs e)
    {
        _presenter.CopyGrid();
    }

    private void FullScan(object sender, RoutedEventArgs e)
    {
        _presenter.GetFullScan();
    }

    private void Paste(object sender, RoutedEventArgs e)
    {
        _presenter.PasteGrid(Clipboard.GetText());
    }

    public override void OnShow()
    {
        _presenter.RefreshStrategies();
    }

    public override void OnQuit()
    {
        
    }
}