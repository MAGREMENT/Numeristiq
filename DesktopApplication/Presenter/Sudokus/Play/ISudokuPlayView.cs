using DesktopApplication.Presenter.Sudokus.Solve;
using Model.Sudokus.Player;

namespace DesktopApplication.Presenter.Sudokus.Play;

public interface ISudokuPlayView : ICanBeDisabled
{
    ISudokuPlayerDrawer Drawer { get; }
    ISudokuSolverDrawer ClueShower { get; }

    void InitializeHighlightColorBoxes();
    void FocusDrawer();
    void SetIsPlaying(bool isPlaying);
    void SetTimeElapsed(TimeQuantity quantity);
    void SetHistoricAvailability(bool canMoveBack, bool canMoveForward);
    void ShowClueState(bool isShowing);
    void ShowClueText(string text);
    void OpenOptionDialog(string name, OptionChosen callback, params string[] options);
}