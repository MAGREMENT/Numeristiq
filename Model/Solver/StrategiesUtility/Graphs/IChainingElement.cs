using Global;
using Model.Solver.Possibility;

namespace Model.Solver.StrategiesUtility.Graphs;

public interface IChainingElement
{
    int Rank { get; }
    Possibilities EveryPossibilities();
    CellPossibilities[] EveryCellPossibilities();
    Cell[] EveryCell();
    CellPossibility[] EveryCellPossibility();
}