using System.Collections.Generic;
using System.Threading.Tasks;
using DesktopApplication.Presenter.Sudoku.Solve.ChooseStep;
using DesktopApplication.Presenter.Sudoku.Solve.Explanation;
using Model;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Explanation;
using Model.Sudoku.Solver.Trackers;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public class SudokuSolvePresenter : ICommitApplier
{
    private readonly ISudokuSolveView _view;
    private readonly SolveActionEnabler _enabler;
    private readonly SudokuHighlighterTranslator _translator;
    private readonly Settings _settings;
    private readonly IStrategyRepositoryUpdater _updater;
    private readonly SudokuSolver _solver;
    
    private ISolvingState? _currentlyDisplayedState;
    private int _currentlyOpenedLog = -1;
    private Cell? _selectedCell;
    private SolveTracker? _solveTracker;

    private int _logCount;
    private StateShown _stateShown = StateShown.Before;

    public SudokuSolvePresenter(ISudokuSolveView view, SudokuSolver solver, Settings settings, IStrategyRepositoryUpdater updater)
    {
        _view = view;
        _enabler = new SolveActionEnabler(_view);
        _translator = new SudokuHighlighterTranslator(_view.Drawer, settings);
        _solver = solver;
        _settings = settings;
        _updater = updater;

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
        
        _solveTracker ??= new SolveTracker(this, true);
        _solveTracker.UpdateLogs = true;
        _solver.AddTracker(_solveTracker);
        await Task.Run(() => _solver.Solve(stopAtProgress));
        _solver.RemoveTracker(_solveTracker);

        _enabler.EnableActions(1);
    }

    public async Task<ChooseStepPresenterBuilder> ChooseStep()
    {
        _enabler.DisableActions(2);
        
        _solveTracker ??= new SolveTracker(this, false);
        _solveTracker.UpdateLogs = false;
        var commits = await Task.Run(() => _solver.EveryPossibleNextStep());
        _solver.RemoveTracker(_solveTracker);

        return new ChooseStepPresenterBuilder(commits, _settings, _solver, this);
    }

    public void OnStoppedChoosingStep()
    {
        _enabler.EnableActions(2);
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
        if (index < 0 || index >= _solver.Logs.Count) return;
        
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
        if (_currentlyOpenedLog < 0 || _currentlyOpenedLog >= _solver.Logs.Count) return;
        
        var log = _solver.Logs[_currentlyOpenedLog];
        SetShownState(_stateShown == StateShown.Before ? log.StateBefore : log.StateAfter, false); 
        _translator.Translate(log.HighlightManager);
    }

    public void RequestHighlightShift(bool isLeft)
    {
        if (_currentlyOpenedLog < 0 || _currentlyOpenedLog >= _solver.Logs.Count) return;
        
        var log = _solver.Logs[_currentlyOpenedLog];
        if(isLeft) log.HighlightManager.ShiftLeft();
        else log.HighlightManager.ShiftRight();
        
        _view.Drawer.ClearHighlights();
        _translator.Translate(log.HighlightManager);
        _view.SetCursorPosition(_currentlyOpenedLog, log.HighlightManager.CursorPosition());
    }
    
    public StepExplanationPresenterBuilder? RequestExplanation()
    {
        if (_currentlyOpenedLog < 0 || _currentlyOpenedLog >= _solver.Logs.Count) return null;
        
        return new StepExplanationPresenterBuilder(_solver.Logs[_currentlyOpenedLog].Explanation, _solver);
    }

    public void EnableStrategy(int index, bool enabled)
    {
        if (index < 0 || index >= _solver.StrategyManager.Strategies.Count) return;

        _solver.StrategyManager.Strategies[index].Enabled = enabled;
        _updater.Update();
    }

    public void SelectCell(int row, int col)
    {
        var c = new Cell(row, col);
        if (_selectedCell is null || _selectedCell.Value != c)
        {
            _selectedCell = c;
            _view.Drawer.PutCursorOn(c);
            _view.Drawer.Refresh();
        }
        else
        {
            _selectedCell = null;
            _view.Drawer.ClearCursor();
            _view.Drawer.Refresh();
        }
    }

    public void SetCurrentCell(int n)
    {
        if (_selectedCell is null) return;
        var c = _selectedCell.Value;
        _solver.SetSolutionByHand(n, c.Row, c.Column);
        SetShownState(_solver, true);
    }

    public void DeleteCurrentCell()
    {
        if (_selectedCell is null) return;
        var c = _selectedCell.Value;
        _solver.RemoveSolutionByHand(c.Row, c.Column);
        SetShownState(_solver, true);
    }

    public void HighlightStrategy(int index)
    {
        _view.HighlightStrategy(index);
    }

    public void UnHighlightStrategy(int index)
    {
        _view.UnHighlightStrategy(index);
    }
    
    public void Apply(BuiltChangeCommit<ISudokuHighlighter> commit)
    {
        _solver.ApplyCommit(commit);
        UpdateLogs();
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
        drawer.ClearHighlights();
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
    
    public bool UpdateLogs { get; set; }

    public SolveTracker(SudokuSolvePresenter presenter, bool updateLogs)
    {
        _presenter = presenter;
        UpdateLogs = updateLogs;
    }

    public override void OnStrategyStart(SudokuStrategy strategy, int index)
    {
        _presenter.HighlightStrategy(index);
    }

    public override void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        _presenter.UnHighlightStrategy(index);
        if (UpdateLogs && solutionAdded + possibilitiesRemoved > 0)
        {
            _presenter.ShowCurrentState();
            _presenter.UpdateLogs();
        }
    }
}

public class ChooseStepPresenterBuilder
{
    private readonly IReadOnlyList<BuiltChangeCommit<ISudokuHighlighter>> _commits;
    private readonly ISolvingState _currentState;
    private readonly Settings _settings;
    private readonly ICommitApplier _applier;

    public ChooseStepPresenterBuilder(IReadOnlyList<BuiltChangeCommit<ISudokuHighlighter>> commits, Settings settings,
        ISolvingState currentState, ICommitApplier applier)
    {
        _commits = commits;
        _settings = settings;
        _currentState = currentState;
        _applier = applier;
    }

    public ChooseStepPresenter Build(IChooseStepView view)
    {
        return new ChooseStepPresenter(view, _currentState, _commits, _applier, _settings);
    }
}

public class StepExplanationPresenterBuilder
{
    private readonly ExplanationElement? _start;
    private readonly ISolvingState _state;

    public StepExplanationPresenterBuilder(ExplanationElement? start, ISolvingState state)
    {
        _start = start;
        _state = state;
    }

    public StepExplanationPresenter Build(IStepExplanationView view)
    {
        return new StepExplanationPresenter(view, _state, _start);
    }
}

public interface ICommitApplier
{
    public void Apply(BuiltChangeCommit<ISudokuHighlighter> commit);
}