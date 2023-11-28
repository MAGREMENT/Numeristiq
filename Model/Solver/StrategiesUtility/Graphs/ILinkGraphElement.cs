using Global;
using Model.Solver.Possibility;

namespace Model.Solver.StrategiesUtility.Graphs;

public interface ILinkGraphElement
{
    int Rank { get; }
    CellPossibilities[] EveryCellPossibilities();
    Cell[] EveryCell();
    Possibilities EveryPossibilities();
}