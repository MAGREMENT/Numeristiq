using Global;
using Global.Enums;
using Model;
using Model.Solver;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Logs;
using Presenter.StepChooser;
using Presenter.Translators;

namespace Presenter.Solver;

public class SolverPresenter : IStepChooserCallback
{
    private readonly ISolver _solver;
    private readonly ISolverView _view;

    private SolverState _shownState;
    private int _lastLogIndex = -1;
    private Cell? _currentlySelectedCell;
    private int _currentlySelectedLog = -1;
    private bool _shouldUpdateSudokuTranslation = true;
    private bool _shouldUpdateLogs = true;

    public ISolverSettings Settings { get; }

    private readonly HighlighterTranslator _highlighterTranslator;
    private readonly SolverActionEnabler _solverActionEnabler;

    public SolverPresenter(ISolver solver, ISolverView view, ISolverSettings settings)
    {
        _solver = solver;
        _view = view;

        _shownState = _solver.CurrentState;

        Settings = settings;
        Settings.ShownStateChanged += () => SelectLog(_currentlySelectedLog);
        Settings.TranslationTypeChanged += () =>
            _view.SetTranslation(SudokuTranslator.TranslateToLine(_shownState, Settings.TranslationType));
        Settings.UniquenessAllowedChanged += () =>
        {
            _solver.AllowUniqueness(Settings.UniquenessAllowed);
            _view.UpdateStrategies(ModelToViewTranslator.Translate(_solver.GetStrategyInfo()));
        };
        Settings.RedrawNeeded += Redraw;
        
        _highlighterTranslator = new HighlighterTranslator(view, Settings);
        _solverActionEnabler = new SolverActionEnabler(view);
    }
    
    //Presenter---------------------------------------------------------------------------------------------------------

    public void Bind()
    {
        _solver.LogsUpdated += UpdateLogs;
        _solver.CurrentStrategyChanged += i => _view.LightUpStrategy(i);

        ChangeShownState(_shownState);
        _view.InitializeStrategies(ModelToViewTranslator.Translate(_solver.GetStrategyInfo()));
    }

    public void RefreshStrategies()
    {
        _view.InitializeStrategies(ModelToViewTranslator.Translate(_solver.GetStrategyInfo()));
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

    public async void Solve(bool stepByStep)
    {
        _solverActionEnabler.DisableActions(1);

        if (!stepByStep) _shouldUpdateLogs = false;
        await Task.Run(() => _solver.Solve(stepByStep));
        _shouldUpdateLogs = true;
        
        _solverActionEnabler.EnableActions(1);
    }

    public async void ChooseNextStep()
    {
        _solverActionEnabler.DisableActions(3);
        
        var buffer = Array.Empty<BuiltChangeCommit>();
        await Task.Run(() => buffer = _solver.EveryPossibleNextStep());
        _view.ShowPossibleSteps(new StepChooserPresenterBuilder(_solver.CurrentState, buffer, this));
    }
    
    public void SelectLog(int number)
    {
        if (number < 0 || number >= _solver.Logs.Count) return;

        _currentlySelectedLog = number;
        var log = _solver.Logs[number];
        _view.FocusLog(number);
        _view.ShowExplanation(log.Explanation);
        ChangeShownState(Settings.StateShown == StateShown.Before ? log.StateBefore : log.StateAfter);
        HighlightLog(log);
    }

    public void ShiftLog(int delta)
    {
        if (_currentlySelectedLog == -1) return;

        SelectLog(_currentlySelectedLog + delta);
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
        _view.Refresh();
    }

    public void ShowCurrentState()
    {
        ClearLogFocus();
        ChangeShownState(_solver.CurrentState);
        _view.ClearDrawings();
        _view.Refresh();
    }

    public void UseStrategy(int number, bool yes)
    {
        if (yes) _solver.UseStrategy(number);
        else _solver.ExcludeStrategy(number);
    }

    public void UseAllStrategies(bool yes)
    {
        _solver.UseAllStrategies(yes);
    }

    public void SelectCell(Cell cell)
    {
        if (_currentlySelectedCell is null || _currentlySelectedCell != cell)
        {
            _currentlySelectedCell = cell;
            _view.PutCursorOn(cell);
            _view.Refresh();
        }
        else
        {
            _currentlySelectedCell = null;
            _view.ClearCursor();
            _view.Refresh();
        }
    }

    public void UnSelectCell()
    {
        _currentlySelectedCell = null;
        _view.ClearCursor();
        _view.Refresh();
    }

    public void ChangeCurrentCell(int number)
    {
        if (_currentlySelectedCell is null) return;

        if (Settings.ActionOnCellChange == ChangeType.Possibility) _solver.RemovePossibilityByHand(number,
                _currentlySelectedCell.Value.Row, _currentlySelectedCell.Value.Column);
        else _solver.SetSolutionByHand(number, _currentlySelectedCell.Value.Row, _currentlySelectedCell.Value.Column);
        ChangeShownState(_solver.CurrentState);
    }

    public void RemoveCurrentCell()
    {
        if (_currentlySelectedCell is not null) _solver.RemoveSolutionByHand(_currentlySelectedCell.Value.Row,
                _currentlySelectedCell.Value.Column);
        ChangeShownState(_solver.CurrentState);
    }

    public void Copy()
    {
        _view.ToClipboard(SudokuTranslator.TranslateToGrid(_shownState));
    }

    public void Paste(string s)
    {
        switch (SudokuTranslator.TryGetFormat(s))
        {
            case SudokuStringFormat.Line :
                _solver.SetSudoku(SudokuTranslator.TranslateToSudoku(s));
                break;
            case SudokuStringFormat.Grid :
                _solver.SetState(SudokuTranslator.TranslateToState(s, Settings.TransformSoloPossibilityIntoGiven));
                break;
        }
        
        ClearLogs();
        ClearLogFocus();
        ChangeShownState(_solver.CurrentState);
        _view.ClearDrawings();
        _view.Refresh();
    }
    
    //IStepChooserCallback----------------------------------------------------------------------------------------------
    
    public void EnableActionsBack()
    {
        _solverActionEnabler.EnableActions(3);
    }

    public void ApplyCommit(BuiltChangeCommit commit)
    {
        _solver.ApplyCommit(commit);
    }

    public CellColor GetCellColor(int row, int col)
    {
        return _solver.StartState[row, col] == 0 ? Settings.SolvingColor : Settings.GivenColor;
    }

    //Private-----------------------------------------------------------------------------------------------------------

    private void ChangeShownState(SolverState state)
    {
        _shownState = state;
       
        _view.ClearNumbers();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var current = state.At(row, col);
                if (current.IsPossibilities) _view.ShowPossibilities(row, col, current.AsPossibilities.ToArray(), GetCellColor(row, col));
                else _view.ShowSolution(row, col, current.AsNumber, GetCellColor(row, col));
            }
        }
        _view.Refresh();
        
        if(_shouldUpdateSudokuTranslation) _view.SetTranslation(SudokuTranslator.TranslateToLine(state, Settings.TranslationType));
    }

    private void Redraw()
    {
        _view.ClearNumbers();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var current = _shownState.At(row, col);
                if (current.IsPossibilities) _view.ShowPossibilities(row, col, current.AsPossibilities.ToArray(), GetCellColor(row, col));
                else _view.ShowSolution(row, col, current.AsNumber, GetCellColor(row, col));
            }
        }
        
        if (_currentlySelectedLog != -1) HighlightLog(_solver.Logs[_currentlySelectedLog]);
        else _view.Refresh();
    }

    private void NewSudoku(Sudoku sudoku)
    {
        _solver.SetSudoku(sudoku);
        
        ClearLogs();
        ClearLogFocus();
        ChangeShownState(_solver.CurrentState);
        _view.ClearDrawings();
        _view.Refresh();
    }

    private async void UpdateLogs()
    {
        _solverActionEnabler.DisableActions(2);
        
        var logs = _solver.Logs;
        _view.SetLogs(ModelToViewTranslator.Translate(logs));

        if (!_shouldUpdateLogs) _lastLogIndex = logs.Count - 1;
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
                _view.Refresh();
            }   
        }
        
        
        ClearLogFocus();
        ChangeShownState(_solver.CurrentState);
        _view.ClearDrawings();
        _view.Refresh();
        _solverActionEnabler.EnableActions(2);
    }

    private void ClearLogs()
    {
        _lastLogIndex = -1;
    }

    private void ClearLogFocus()
    {
        _currentlySelectedLog = -1;
        _view.UnFocusLog();
        _view.ShowExplanation("");
    }

    private void HighlightLog(ISolverLog log)
    {
        _view.ClearDrawings();
        _highlighterTranslator.Translate(log.HighlightManager);
        _view.Refresh();
    }
}