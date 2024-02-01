namespace Presenter.Sudoku.Player;

public interface IPlayerView : IPlayerDrawer
{
    void SetChangeMode(ChangeMode mode);
    void SetLocationMode(LocationMode mode);
    void SetMoveAvailability(bool canMoveBack, bool canMoveForward);
}