using Model.Helpers;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public interface ISudokuSolveView
{
    void SetSudokuAsString(string s);
    void DisplaySudoku(ITranslatable translatable);
}