using Model;
using Model.Helpers.Logs;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public interface ISudokuSolveView
{
    ISudokuDrawer Drawer { get; }
    
    void SetSudokuAsString(string s);
    void DisableSolveActions();
    void EnableSolveActions();
    void AddLog(ISolverLog log, StateShown _shown);
    void ClearLogs();
    void OpenLog(int index);
    void CloseLogs();
    void SetLogsStateShown(StateShown stateShown);
    void SetCursorPosition(int index, string s);
}