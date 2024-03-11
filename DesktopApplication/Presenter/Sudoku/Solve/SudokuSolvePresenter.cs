using System.Threading.Tasks;
using Model;
using Model.Helpers;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public class SudokuSolvePresenter
{
    private readonly ISudokuSolveView _view;
    private readonly SolveActionEnabler _enabler;
    private readonly SudokuHighlighterTranslator _translator;

    private readonly SudokuSolver _solver;
    private ISolvingState? _currentlyDisplayedState;
    private int _currentlyOpenedLog = -1;
    private SolveTracker? _solveTracker;

    private int _logCount;
    private StateShown _stateShown = StateShown.Before;

    public SudokuSolvePresenter(ISudokuSolveView view, SudokuSolver solver, Settings settings)
    {
        _view = view;
        _enabler = new SolveActionEnabler(_view);
        _translator = new SudokuHighlighterTranslator(_view.Drawer, settings);
        _solver = solver;

        _view.InitializeStrategies(_solver.StrategyManager.Strategies);
    }

    public void OnSudokuAsStringBoxShowed()
    {
        _view.SetSudokuAsString(SudokuTranslator.TranslateLineFormat(_solver.Sudoku, SudokuTranslationType.Shortcuts));
    }

    public void SetNewSudoku(string s)
    {
        _solver.SetSudoku(SudokuTranslator.TranslateLineFormat(s));
        SetShownState(_solver, true);
        ClearLogs();
    }

    public async void Solve(bool stopAtProgress)
    {
        _enabler.DisableActions(1);
        
        _solveTracker ??= new SolveTracker(this);
        _solver.AddTracker(_solveTracker);
        await Task.Run(() => _solver.Solve(stopAtProgress));
        _solver.RemoveTracker(_solveTracker);

        _enabler.EnableActions(1);
    }

    public void Clear()
    {
        _solver.SetSudoku(new Model.Sudoku.Sudoku());
        SetShownState(_solver.StartState, true);
        ClearLogs();
    }

    public void ShowCurrentState()
    {
        SetShownState(_solver, false);
    }

    public void UpdateLogs()
    {
        if (_solver.Logs.Count < _logCount)
        {
            ClearLogs();
            return;
        }

        for (;_logCount < _solver.Logs.Count; _logCount++)
        {
            _view.AddLog(_solver.Logs[_logCount], _stateShown);
        }
    }

    public void RequestLogOpening(int id)
    {
        var index = id - 1;
        if (index < 0 || index > _solver.Logs.Count) return;
        
        _view.CloseLogs();

        if (_currentlyOpenedLog == index)
        {
            _currentlyOpenedLog = -1;
            SetShownState(_solver, false);
        }
        else
        {
            _view.OpenLog(index);
            _currentlyOpenedLog = index;

            var log = _solver.Logs[index];
            SetShownState(_stateShown == StateShown.Before ? log.StateBefore : log.StateAfter, false); 
            _translator.Translate(log.HighlightManager); 
        }
    }

    public void RequestStateShownChange(StateShown ss)
    {
        _stateShown = ss;
        _view.SetLogsStateShown(ss);
        if (_currentlyOpenedLog < 0 || _currentlyOpenedLog > _solver.Logs.Count) return;
        
        var log = _solver.Logs[_currentlyOpenedLog];
        SetShownState(_stateShown == StateShown.Before ? log.StateBefore : log.StateAfter, false); 
        _translator.Translate(log.HighlightManager);
    }

    public void RequestHighlightShift(bool isLeft)
    {
        if (_currentlyOpenedLog < 0 || _currentlyOpenedLog > _solver.Logs.Count) return;
        
        var log = _solver.Logs[_currentlyOpenedLog];
        if(isLeft) log.HighlightManager.ShiftLeft();
        else log.HighlightManager.ShiftRight();
        
        _view.Drawer.ClearHighlightings();
        _translator.Translate(log.HighlightManager);
        _view.SetCursorPosition(_currentlyOpenedLog, log.HighlightManager.CursorPosition());
    }

    public void EnableStrategy(int index, bool enabled)
    {
        if (index < 0 || index >= _solver.StrategyManager.Strategies.Count) return;

        _solver.StrategyManager.Strategies[index].Enabled = enabled;
    }

    private void ClearLogs()
    {
        _view.ClearLogs();
        _logCount = 0;
    }

    private void SetShownState(ISolvingState solvingState, bool solutionAsClues)
    {
        _currentlyDisplayedState = solvingState;
        var drawer = _view.Drawer;
        
        drawer.ClearNumbers();
        drawer.ClearHighlightings();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var number = solvingState[row, col];
                if (number == 0)
                {
                    if(solutionAsClues) drawer.SetClue(row, col, false);
                    drawer.ShowPossibilities(row, col, solvingState.PossibilitiesAt(row, col).EnumeratePossibilities());
                }
                else
                {
                    if (solutionAsClues) drawer.SetClue(row, col, true);
                    drawer.ShowSolution(row, col, number);
                }
            }
        }
        
        drawer.Refresh();
    }
}

public class SolveTracker : Tracker
{
    private readonly SudokuSolvePresenter _presenter;

    public SolveTracker(SudokuSolvePresenter presenter)
    {
        _presenter = presenter;
    }

    public override void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved > 0)
        {
            _presenter.ShowCurrentState();
            _presenter.UpdateLogs();
        }
    }
}