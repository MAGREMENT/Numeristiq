using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Player;

namespace Model;

public interface IPlayer : IPlayerState
{
    public event OnChange? Changed;
    public event OnMoveAvailabilityChange? MoveAvailabilityChanged;

    public void SetNumber(int number, IEnumerable<Cell> cells);
    public void RemoveNumber(int number, IEnumerable<Cell> cells);
    public void RemoveNumber(IEnumerable<Cell> cells);
    public void AddPossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells);
    public void RemovePossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells);
    public void RemovePossibility(PossibilitiesLocation location, IEnumerable<Cell> cells);
    public void Highlight(HighlightColor color, IEnumerable<Cell> cells);
    public void MoveBack();
    public void MoveForward();
}

public delegate void OnChange();