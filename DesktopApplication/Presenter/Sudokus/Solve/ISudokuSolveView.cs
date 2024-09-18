using System.Collections.Generic;
using Model.Sudokus.Solver;

namespace DesktopApplication.Presenter.Sudokus.Solve;

public interface ISudokuSolveView : ICanBeDisabled, ISolveWithStepsView
{
    ISudokuSolverDrawer Drawer { get; }
    
    void SetSudokuAsString(string s);
    void InitializeStrategies(IReadOnlyList<SudokuStrategy> strategies);
    void HighlightStrategy(int index);
    void UnHighlightStrategy(int index);
    void CopyToClipBoard(string s);
    void EnableStrategy(int index, bool enabled);
    void LockStrategy(int index);
    void OpenOptionDialog(string name, OptionChosen callback, OptionCollection collection);
}

public delegate void OptionChosen(int n);
public enum OptionCollection
{
    SudokuStringFormat, SudokuSolverCopy
}

