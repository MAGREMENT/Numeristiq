using System.Collections.Generic;
using Model.Helpers.Highlighting;
using Model.Helpers.Logs;
using Model.Sudokus.Solver;

namespace DesktopApplication.Presenter.Sudokus.Solve;

public interface ISudokuSolveView
{
    ISudokuSolverDrawer Drawer { get; }
    
    void SetSudokuAsString(string s);
    void DisableSolveActions();
    void EnableSolveActions();
    void AddLog(ISolverLog<ISudokuHighlighter> log, StateShown _shown);
    void ClearLogs();
    void OpenLog(int index);
    void CloseLog(int index);
    void SetLogsStateShown(StateShown stateShown);
    void SetCursorPosition(int index, string s);
    void InitializeStrategies(IReadOnlyList<SudokuStrategy> strategies);
    void HighlightStrategy(int index);
    void UnHighlightStrategy(int index);
    void CopyToClipBoard(string s);
    void EnableStrategy(int index, bool enabled);
    void LockStrategy(int index);
    void OpenOptionDialog(string name, OptionChosen callback, params string[] options);
}

public delegate void OptionChosen(int n);

