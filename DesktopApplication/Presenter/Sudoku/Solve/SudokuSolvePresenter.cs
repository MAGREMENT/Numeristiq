using System.Threading.Tasks;
using Model.Helpers;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public class SudokuSolvePresenter
{
    private readonly ISudokuSolveView _view;
    private readonly SolveActionEnabler _enabler;

    private readonly SudokuSolver _solver;
    private ITranslatable? _shownState;
    private SolveTracker? _solveTracker;

    private int _logCount;

    public SudokuSolvePresenter(ISudokuSolveView view, SudokuSolver solver)
    {
        _view = view;
        _enabler = new SolveActionEnabler(_view);
        _solver = solver;
    }

    public void OnSudokuAsStringBoxShowed()
    {
        _view.SetSudokuAsString(SudokuTranslator.TranslateLineFormat(_solver.Sudoku, SudokuTranslationType.Shortcuts));
    }

    public void SetNewSudoku(string s)
    {
        _solver.SetSudoku(SudokuTranslator.TranslateLineFormat(s));
        ShowCurrentState();
        _view.SetClues(_solver.StartState);
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
        ShowCurrentState();
        _view.SetClues(_solver.StartState);
        ClearLogs();
    }

    public void ShowCurrentState()
    {
        SetShownState(_solver.CurrentState);
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
            _view.AddLog(_solver.Logs[_logCount]);
        }
    }

    private void ClearLogs()
    {
        _view.ClearLogs();
        _logCount = 0;
    }

    private void SetShownState(ITranslatable translatable)
    {
        _shownState = translatable;
        _view.DisplaySudoku(translatable);
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