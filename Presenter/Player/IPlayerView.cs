using Global;

namespace Presenter.Player;

public interface IPlayerView : IPlayerDrawer
{
    void SetChangeMode(ChangeMode mode);
    void SetLocationMode(LocationMode mode);
    void SetMoveAvailability(bool canMoveBack, bool canMoveForward);
}