using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Player;

namespace Model;

public interface IPlayer
{
    public event OnChange? Changed;

    public void SetNumber(int number, IEnumerable<Cell> cells);
    public void RemoveNumber(int number, IEnumerable<Cell> cells);
    public void AddPossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells);
    public void RemovePossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells);
    
    public PlayerCell this[int row, int column] { get; }
}

public delegate void OnChange();