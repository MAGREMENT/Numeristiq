using Global.Enums;

namespace Presenter.Player;

public interface IPlayerSettings
{
    public event OnSettingChange? MultiColorHighlightingChanged;
    public event OnSettingChange? RedrawNeeded;
    
    public CellColor GivenColor { get; set; }
    public CellColor SolvingColor { get; set; }
    public bool TransformSoloPossibilityIntoGiven { get; set; }
    public bool MultiColorHighlighting { get; set; }
    public int StartAngle { get; set; }
    public RotationDirection RotationDirection { get; set; }
}