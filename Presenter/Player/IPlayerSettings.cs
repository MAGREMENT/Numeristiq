using Global.Enums;

namespace Presenter.Player;

public interface IPlayerSettings
{
    public CellColor GivenColor { get; }
    public CellColor SolvingColor { get; }
    public bool TransformSoloPossibilityIntoGiven { get; }
}