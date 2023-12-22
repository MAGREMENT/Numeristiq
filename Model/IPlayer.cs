using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Player;
using Model.Solver;

namespace Model;

public interface IPlayer : IPlayerState
{
    public bool MultiColorHighlighting { set; }
    
    public event OnChange? Changed;
    public event OnMoveAvailabilityChange? MoveAvailabilityChanged;

    public void SetNumber(int number, IEnumerable<Cell> cells);
    public void RemoveNumber(int number, IEnumerable<Cell> cells);
    public void RemoveNumber(IEnumerable<Cell> cells);
    public void AddPossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells);
    public void RemovePossibility(int possibility, PossibilitiesLocation location, IEnumerable<Cell> cells);
    public void RemovePossibility(PossibilitiesLocation location, IEnumerable<Cell> cells);
    public void ClearNumbers(IEnumerable<Cell> cells);
    public void ClearNumbers();
    public void ComputeDefaultPossibilities(IEnumerable<Cell> cells);
    public void ComputeDefaultPossibilities();
    public void Highlight(HighlightColor color, IEnumerable<Cell> cells);
    public void ClearHighlights(IEnumerable<Cell> cells);
    public void ClearHighlights();
    public void MoveBack();
    public void MoveForward();
    public void Paste(Sudoku s);
    public void Paste(SolverState ss);
}

public delegate void OnChange();