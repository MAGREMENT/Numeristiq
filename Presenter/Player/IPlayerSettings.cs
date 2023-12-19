using Global.Enums;

namespace Presenter.Player;

public interface IPlayerSettings
{
    public CellColor GivenColor { get; set; }
    public CellColor SolvingColor { get; set; }
}