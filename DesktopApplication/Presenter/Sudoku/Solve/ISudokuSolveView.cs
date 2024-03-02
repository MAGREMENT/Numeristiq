using Model.Helpers;
using Model.Helpers.Logs;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public interface ISudokuSolveView
{
    void SetSudokuAsString(string s);
    void DisplaySudoku(ITranslatable translatable);
    void SetClues(ITranslatable translatable);
    void DisableSolveActions();
    void EnableSolveActions();
    void AddLog(ISolverLog log);
    void ClearLogs();
}