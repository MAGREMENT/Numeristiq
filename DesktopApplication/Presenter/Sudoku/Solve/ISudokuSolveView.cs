using System.Collections.Generic;
using Model;
using Model.Helpers.Highlighting;
using Model.Helpers.Logs;
using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public interface ISudokuSolveView
{
    ISudokuDrawer Drawer { get; }
    
    void SetSudokuAsString(string s);
    void DisableSolveActions();
    void EnableSolveActions();
    void AddLog(ISolverLog<ISudokuHighlighter> log, StateShown _shown);
    void ClearLogs();
    void OpenLog(int index);
    void CloseLogs();
    void SetLogsStateShown(StateShown stateShown);
    void SetCursorPosition(int index, string s);
    void InitializeStrategies(IReadOnlyList<SudokuStrategy> strategies);
}