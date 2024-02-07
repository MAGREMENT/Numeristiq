using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Model.Helpers.Changes;
using Model.Helpers.Logs;
using Model.Sudoku;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;
using Presenter;
using Presenter.Sudoku;
using Presenter.Sudoku.Solver;
using Presenter.Sudoku.Translators;
using View.HelperWindows.Explanation;
using View.HelperWindows.Settings;
using View.HelperWindows.StepChooser;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace View.Pages.Solver;

public partial class SolverPage : ISolverView
{
    private bool _createNewSudoku = true;

    private readonly SolverPresenter _presenter;
    private readonly IPageHandler _pageHandler;

    private readonly StepChooserWindow _stepChooserWindow;
    private readonly SettingsWindow _settingsWindow;
    private readonly ExplanationWindow _explanationWindow;

    public SolverPage(IPageHandler pageHandler, ApplicationPresenter factory)
    {
        InitializeComponent();

        Focusable = true;

        _presenter = factory.Create(this);
        _presenter.Bind();
        _pageHandler = pageHandler;

        LogViewer.SetShownType(_presenter.Settings.StateShown);

        Solver.CellSelected += _presenter.SelectCell;
        Solver.CellUnselected += _presenter.UnSelectCell;
        Solver.CurrentCellChangeAsked += _presenter.ChangeCurrentCell;
        Solver.RemoveSolutionFromCurrentCellAsked += _presenter.RemoveCurrentCell;
        LogList.LogSelected += _presenter.SelectLog;
        LogList.LogShifted += _presenter.ShiftLog;
        LogList.ShowStartStateAsked += _presenter.ShowStartState;
        LogList.ShowCurrentStateAsked += _presenter.ShowCurrentState;
        LogViewer.StateShownChanged += ss => _presenter.Settings.StateShown = ss;
        LogViewer.LogHighlightShifted += _presenter.ShiftLogHighlight;
        LogViewer.ExplanationAsked += _presenter.ShowExplanation;
        StrategyList.StrategyUsed += _presenter.UseStrategy;
        StrategyList.AllStrategiesUsed += _presenter.UseAllStrategies;

        _stepChooserWindow = new StepChooserWindow();
        _settingsWindow = SettingsWindow.From(_presenter.Settings);
        _explanationWindow = new ExplanationWindow();
    }
    
    //ISolverView-------------------------------------------------------------------------------------------------------

    public void DisableActions()
    {
        Dispatcher.Invoke(() =>
        {
            SolveButton.IsEnabled = false;
            ClearButton.IsEnabled = false;
            ChooseButton.IsEnabled = false;
            AdvanceButton.IsEnabled = false;
        });
    }

    public void EnableActions()
    {
        Dispatcher.Invoke(() =>
        {
            SolveButton.IsEnabled = true;
            ClearButton.IsEnabled = true;
            ChooseButton.IsEnabled = true;
            AdvanceButton.IsEnabled = true;
        });
    }

    public void ClearNumbers()
    {
        Dispatcher.Invoke(() => Solver.ClearNumber());
    }

    public void ShowSolution(int row, int col, int number, CellColor color)
    { 
        
        Solver.Dispatcher.Invoke(() => Solver.SetCellTo(row, col, number, color));
    }

    public void ShowPossibilities(int row, int col, int[] possibilities, CellColor color)
    {
        Solver.Dispatcher.Invoke(() => Solver.SetCellTo(row, col, possibilities, color));
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
        LogViewer.Dispatcher.Invoke(() => LogViewer.StopShowing());
    }

    public void SetLogs(IReadOnlyList<ViewLog> logs)
    {
        LogList.Dispatcher.Invoke(() => LogList.SetLogs(logs));
    }

    public void ShowFocusedLog(ViewLog log)
    {
        LogViewer.Dispatcher.Invoke(() => LogViewer.Show(log));
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

    public void StopLightingUpStrategy(int number)
    {
        StrategyList.Dispatcher.Invoke(() => StrategyList.StopLightingUpStrategy(number));
    }

    public void PutCursorOn(Cell cell)
    {
        Solver.Dispatcher.Invoke(() => Solver.PutCursorOn(cell));
    }

    public void ClearCursor()
    {
        Solver.Dispatcher.Invoke(Solver.ClearCursor);
    }

    public void Refresh()
    {
        Solver.Dispatcher.Invoke(Solver.Refresh);
    }

    public void ToClipboard(string s)
    {
        Clipboard.SetText(s);
    }

    public void ShowPossibleSteps(StepChooserPresenterBuilder builder)
    {
        _stepChooserWindow.SetPresenter(builder);
        _stepChooserWindow.Show();
    }

    public void ShowExplanation(ISolverLog log)
    {
        _explanationWindow.ShowExplanation(log);
        _explanationWindow.Show();
    }

    public void ClearDrawings()
    {
        Solver.Dispatcher.Invoke(Solver.ClearDrawings);
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

    public void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, ChangeColoration coloration)
    {
        Solver.Dispatcher.Invoke(() => Solver.EncircleRectangle(rowFrom, colFrom, rowTo, colTo, coloration));
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
        _presenter.Solve(false);
    }

    private void ClearSudoku(object sender, RoutedEventArgs e)
    {
        _presenter.ClearSudoku();
    }
    
    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private void ChooseStep(object sender, RoutedEventArgs e)
    {
        _presenter.ChooseNextStep();
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        _pageHandler.ShowPage(PagesName.First);
    }

    private void ShowSettingsWindow(object sender, RoutedEventArgs e)
    {
        _settingsWindow.Refresh();
        _settingsWindow.Show();
    }

    private void Copy(object sender, RoutedEventArgs e)
    {
        _presenter.Copy();
    }

    private void Paste(object sender, RoutedEventArgs e)
    {
        _presenter.Paste(Clipboard.GetText());
    }

    public override void OnShow()
    {
        _presenter.RefreshStrategies();
        Focus();
    }

    public override void OnQuit()
    {
        
    }

    private void AnalyseKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers != ModifierKeys.Control) return;

        switch (e.Key)
        {
            case Key.X :
            case Key.C :
                _presenter.Copy();
                break;
            case Key.V :
                _presenter.Paste(Clipboard.GetText());
                break;
            case Key.S :
                TakeScreenShot(null, new RoutedEventArgs());
                break;
        }
    }

    private void TakeScreenShot(object? sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "png",
            RestoreDirectory = true,
            Filter = "PNG Image (*.png)|*.png"
        };
        var result = dialog.ShowDialog();

        if (result is DialogResult.OK)
        {
            var stream = dialog.OpenFile();
            Solver.TakeScreenShot(stream);
            stream.Close();
        }
    }
}