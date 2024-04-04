using DesktopApplication.Presenter.Sudoku.Solve;
using Model.Sudoku.Player;

namespace DesktopApplication.Presenter.Sudoku.Play;

public interface ISudokuPlayView
{
    ISudokuPlayerDrawer Drawer { get; }
    ISudokuSolverDrawer ClueShower { get; }

    void FocusDrawer();
    void SetChangeLevelOptions(string[] options, int value);
    void SetIsPlaying(bool isPlaying);
    void SetTimeElapsed(TimeQuantity quantity);
    void SetHistoricAvailability(bool canMoveBack, bool canMoveForward);
    void ShowClueState(bool isShowing);
    void ShowClueText(string text);
    void OpenOptionDialog(string name, OptionChosen callback, params string[] options);
}