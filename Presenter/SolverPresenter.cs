using Global;
using Global.Enums;
using Model;
using Model.Solver;
using Model.Solver.Helpers.Logs;
using Presenter.Translator;

namespace Presenter;

public class SolverPresenter
{
    private readonly ISolver _solver;
    private readonly ISolverView _view;

    private bool _bound;
    private SolverState _shownState;
    private int _lastLogIndex = -1;
    private Cell? _currentlySelectedCell;
    private int _currentlySelectedLog;
    private bool _shouldUpdateSudokuTranslation = true;

    public SolverSettings Settings { get; }

    private readonly HighlighterTranslator _highlighterTranslator;
    private readonly SolverActionEnabler _solverActionEnabler;

    private SolverPresenter(ISolver solver, ISolverView view)
    {
        _solver = solver;
        _view = view;

        _shownState = _solver.CurrentState;
        _highlighterTranslator = new HighlighterTranslator(view);
        _solverActionEnabler = new SolverActionEnabler(view);

        Settings = new SolverSettings();
        Settings.ShownStateChanged += () => SelectLog(_currentlySelectedLog);
        Settings.TranslationTypeChanged += () =>
            _view.SetTranslation(SudokuTranslator.TranslateToLine(_shownState, Settings.TranslationType));
        Settings.UniquenessAllowedChanged += () =>
        {
            _solver.AllowUniqueness(Settings.UniquenessAllowed);
            _view.UpdateStrategies(ModelToViewTranslator.Translate(_solver.StrategyInfos));
        };
        Settings.OnInstanceFoundChanged += () => _solver.SetOnInstanceFound(Settings.OnInstanceFound);
        Settings.GivensNeedUpdate += UpdateGivens;
    }

    public static SolverPresenter FromView(ISolverView view)
    {
        return new SolverPresenter(new SudokuSolver
        {
            LogsManaged = true,
            StatisticsTracked = false
        }, view);
    }

    public void Bind()
    {
        _bound = true;

        _solver.LogsUpdated += UpdateLogs;
        _solver.CurrentStrategyChanged += i => _view.LightUpStrategy(i);

        ChangeShownState(_shownState);
        _view.InitializeStrategies(ModelToViewTranslator.Translate(_solver.StrategyInfos));
    }

    public void NewSudokuFromString(string s)
    {
        _shouldUpdateSudokuTranslation = false;
        NewSudoku(SudokuTranslator.TranslateToSudoku(s));
        _shouldUpdateSudokuTranslation = true;
    }

    public void ClearSudoku()
    {
        NewSudoku(new Sudoku());
    }

    public async void Solve()
    {
        _solverActionEnabler.DisableActions(1);
        await Task.Run(() => _solver.Solve(Settings.StepByStep));
        _solverActionEnabler.EnableActions(1);
    }
    
    public void SelectLog(int number)
    {
        var logs = _solver.Logs;
        if (number < 0 || number >= logs.Count) return;

        _currentlySelectedLog = number;
        var log = logs[number];
        _view.FocusLog(number);
        _view.ShowExplanation(log.Explanation);
        ChangeShownState(Settings.StateShown == StateShown.Before ? log.StateBefore : log.StateAfter);
        HighlightLog(log);
    }

    public void ShiftLogHighlight(int number, int shift)
    {
        var logs = _solver.Logs;
        if (number < 0 || number >= logs.Count) return;

        var log = logs[number];
        if(shift < 0) log.HighlightManager.ShiftLeft();
        else log.HighlightManager.ShiftRight();

        if (number == _currentlySelectedLog)
        {
            HighlightLog(log);
            _view.UpdateFocusedLog(ModelToViewTranslator.Translate(log));
        }
    }

    public void ShowStartState()
    {
        ClearLogFocus();
        ChangeShownState(_solver.StartState);
        _view.ClearDrawings();
        _view.UpdateBackground();
    }

    public void ShowCurrentState()
    {
        ClearLogFocus();
        ChangeShownState(_solver.CurrentState);
        _view.ClearDrawings();
        _view.UpdateBackground();
    }

    public void UseStrategy(int number, bool yes)
    {
        if (yes) _solver.UseStrategy(number);
        else _solver.ExcludeStrategy(number);
    }

    public void SelectCell(Cell cell)
    {
        if (_currentlySelectedCell is null || _currentlySelectedCell != cell)
        {
            _currentlySelectedCell = cell;
            _view.PutCursorOn(cell);
            _view.UpdateBackground();
        }
        else
        {
            _currentlySelectedCell = null;
            _view.ClearCursor();
            _view.UpdateBackground();
        }
    }

    public void UnSelectCell()
    {
        _currentlySelectedCell = null;
        _view.ClearCursor();
        _view.UpdateBackground();
    }

    public void ChangeCurrentCell(int number)
    {
        if (_currentlySelectedCell is null) return;

        if (Settings.ActionOnCellChange == ChangeType.Possibility) _solver.RemovePossibilityByHand(number,
                _currentlySelectedCell.Value.Row, _currentlySelectedCell.Value.Col);
        else _solver.SetSolutionByHand(number, _currentlySelectedCell.Value.Row, _currentlySelectedCell.Value.Col);
        ChangeShownState(_solver.CurrentState);
    }

    public void RemoveCurrentCell()
    {
        if (_currentlySelectedCell is not null) _solver.RemoveSolutionByHand(_currentlySelectedCell.Value.Row,
                _currentlySelectedCell.Value.Col);
        ChangeShownState(_solver.CurrentState);
    }

    public void CopyGrid()
    {
        _view.ToClipboard(SudokuTranslator.TranslateToGrid(_shownState));
    }

    public void PasteGrid(string grid)
    {
        _solver.SetState(SudokuTranslator.TranslateToState(grid));
        ChangeShownState(_solver.CurrentState);
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private void ChangeShownState(SolverState state)
    {
        _shownState = state;
        if (!_bound) return;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var current = state.At(row, col);
                if (current.IsPossibilities) _view.SetCellTo(row, col, current.AsPossibilities.ToArray());
                else _view.SetCellTo(row, col, current.AsNumber);
            }
        }
        
        if(_shouldUpdateSudokuTranslation) _view.SetTranslation(SudokuTranslator.TranslateToLine(state, Settings.TranslationType));
    }

    private void NewSudoku(Sudoku sudoku)
    {
        _solver.SetSudoku(sudoku);
        if (!_bound) return;
        
        ClearLogs();
        ClearLogFocus();
        ChangeShownState(_solver.CurrentState);
        UpdateGivens();
        _view.ClearDrawings();
        _view.UpdateBackground();
    }

    private void UpdateGivens()
    {
        HashSet<Cell> givens = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (_solver.StartState[row, col] != 0) givens.Add(new Cell(row, col));
            }
        }

        _view.UpdateGivens(givens);
    }

    private async void UpdateLogs()
    {
        _solverActionEnabler.DisableActions(2);
        
        var logs = _solver.Logs;
        _view.SetLogs(ModelToViewTranslator.Translate(logs));

        if (!Settings.StepByStep) _lastLogIndex = logs.Count - 1;
        else
        {
            for (int i = _lastLogIndex + 1; i < logs.Count; i++)
            {
                _lastLogIndex = i;
                var current = logs[i];
                if (!current.FromSolving) continue;

                _view.ShowExplanation(current.Explanation);
                _view.FocusLog(i);

                ChangeShownState(current.StateBefore);
                HighlightLog(current);

                await Task.Delay(TimeSpan.FromMilliseconds(Settings.DelayBeforeTransition));

                ChangeShownState(current.StateAfter);

                await Task.Delay(TimeSpan.FromMilliseconds(Settings.DelayAfterTransition));

                _view.ClearDrawings();
                _view.UpdateBackground();
            }   
        }
        
        ClearLogFocus();
        ChangeShownState(_solver.CurrentState);
        _solverActionEnabler.EnableActions(2);
    }

    private void ClearLogs()
    {
        _lastLogIndex = -1;
    }

    private void ClearLogFocus()
    {
        _view.UnFocusLog();
        _view.ShowExplanation("");
    }

    private void HighlightLog(ISolverLog log)
    {
        _view.ClearDrawings();
        _highlighterTranslator.Translate(log.HighlightManager);
        _view.UpdateBackground();
    }
}