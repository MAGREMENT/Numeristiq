using DesktopApplication.Presenter.Sudoku.Solve;
using Model.Sudoku.Player;
using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Play;

public interface ISudokuPlayView
{
    ISudokuPlayerDrawer Drawer { get; }

    void SetChangeLevelOptions(string[] options, int value);
    void SetIsPlaying(bool isPlaying);
    void SetTimeElapsed(TimeQuantity quantity);
    void SetHistoricAvailability(bool canMoveBack, bool canMoveForward);
    void ShowClue(SudokuClue? clue);
    void OpenOptionDialog(string name, OptionChosen callback, params string[] options);
}