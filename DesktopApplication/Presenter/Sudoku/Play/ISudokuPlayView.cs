using Model.Sudoku.Player;

namespace DesktopApplication.Presenter.Sudoku.Play;

public interface ISudokuPlayView
{
    ISudokuPlayerDrawer Drawer { get; }

    void SetChangeLevelOptions(string[] options, int value);
    void SetIsPlaying(bool isPlaying);
    void SetTimeElapsed(TimeQuantity quantity);
    void SetHistoricAvailability(bool canMoveBack, bool canMoveForward);
}