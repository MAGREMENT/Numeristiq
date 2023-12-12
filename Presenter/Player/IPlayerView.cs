using Global;

namespace Presenter.Player;

public interface IPlayerView
{
    void PutCursorOn(HashSet<Cell> cells);
    void ClearCursor();
    void Refresh();
}