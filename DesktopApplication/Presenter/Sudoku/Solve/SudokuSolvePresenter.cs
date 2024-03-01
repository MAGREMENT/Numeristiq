using System.Threading.Tasks;
using Model.Helpers;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public class SudokuSolvePresenter
{
    private readonly ISudokuSolveView _view;

    private readonly SudokuSolver _solver;
    private ITranslatable? _shownState;

    public SudokuSolvePresenter(ISudokuSolveView view, SudokuSolver solver)
    {
        _view = view;
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
    }

    public async void FullSolve()
    {
        var tracker = new FullSolveTracker(this);
        _solver.AddTracker(tracker);
        await Task.Run(() => _solver.Solve());
        _solver.RemoveTracker(tracker);
    }

    public void ShowCurrentState()
    {
        SetShownState(_solver.CurrentState);
    }

    private void SetShownState(ITranslatable translatable)
    {
        _shownState = translatable;
        _view.DisplaySudoku(translatable);
    }
}

public class FullSolveTracker : Tracker
{
    private readonly SudokuSolvePresenter _presenter;

    public FullSolveTracker(SudokuSolvePresenter presenter)
    {
        _presenter = presenter;
    }

    public override void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if(solutionAdded + possibilitiesRemoved > 0) _presenter.ShowCurrentState();
    }
}