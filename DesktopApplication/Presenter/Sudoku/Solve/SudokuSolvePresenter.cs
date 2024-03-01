using Model.Sudoku;
using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public class SudokuSolvePresenter
{
    private readonly ISudokuSolveView _view;

    private readonly SudokuSolver _solver;

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
        _view.DisplaySudoku(_solver.CurrentState);
    }
}