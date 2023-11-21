using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Global;
using Global.Enums;
using Presenter;
using Presenter.Translator;
using View.Settings;
using View.Utility;

namespace View.Pages.Solver;

public partial class SolverPage : ISolverView, ISolverOptionHandler
{
    private bool _createNewSudoku = true;

    private readonly SolverPresenter _presenter;
    private readonly IPageHandler _pageHandler;

    public SolverPage(IPageHandler pageHandler)
    {
        InitializeComponent();

        _presenter = SolverPresenter.FromView(this);
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

    public void UpdateGivens(HashSet<Cell> givens)
    {
        Solver.Dispatcher.Invoke(() => Solver.UpdateGivens(givens));
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
        LinkStrength strength)
    {
        Solver.Dispatcher.Invoke(() => Solver.CreateLink(rowFrom, colFrom, possibilityFrom, rowTo, colTo,
            possibilityTo, strength));
    }

    //ISolverOptionHandler----------------------------------------------------------------------------------------------

    public int DelayBeforeTransition
    {
        get => _presenter.Settings.DelayBeforeTransition;

        set => _presenter.Settings.DelayBeforeTransition = value;
    }

    public int DelayAfterTransition
    {
        get => _presenter.Settings.DelayAfterTransition;

        set => _presenter.Settings.DelayAfterTransition = value;
    }

    public SudokuTranslationType TranslationType
    {
        get => _presenter.Settings.TranslationType;
        set => _presenter.Settings.TranslationType = value;
    }

    public bool StepByStep
    {
        get => _presenter.Settings.StepByStep;
        set => _presenter.Settings.StepByStep = value;
    }

    public bool UniquenessAllowed
    {
        get => _presenter.Settings.UniquenessAllowed;
        set => _presenter.Settings.UniquenessAllowed = value;
    }

    public OnInstanceFound OnInstanceFound
    {
        get => _presenter.Settings.OnInstanceFound;
        set => _presenter.Settings.OnInstanceFound = value;
    }

    public ChangeType ActionOnKeyboardInput
    {
        get => _presenter.Settings.ActionOnCellChange;
        set => _presenter.Settings.ActionOnCellChange = value;
    }

    public Brush GivenForegroundColor
    {
        get => ColorManager.GetInstance().GivenForegroundColor;
        set
        {
            ColorManager.GetInstance().GivenForegroundColor = value;
            _presenter.Settings.NotifyGivensNeedingUpdate();
        }
    }
    public Brush SolvingForegroundColor
    {
        get => ColorManager.GetInstance().SolvingForegroundColor;
        set
        {
            ColorManager.GetInstance().SolvingForegroundColor = value;
            _presenter.Settings.NotifyGivensNeedingUpdate();
        } 
    }

    public LinkOffsetSidePriority SidePriority
    {
        get => Solver.SidePriority;
        set => Solver.SidePriority = value;
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
        var settingsWindow = new SolverSettingsWindow(this);
        settingsWindow.Show();
    }

    private void Copy(object sender, RoutedEventArgs e)
    {
        _presenter.CopyGrid();
    }

    private void FullScan(object sender, RoutedEventArgs e)
    {
        
    }

    private void Paste(object sender, RoutedEventArgs e)
    {
        _presenter.PasteGrid(Clipboard.GetText());
    }
}