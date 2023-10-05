using Model.Solver.Possibilities;

namespace Model.Solver.StrategiesUtil.LinkGraph;

public interface ILinkGraphElement
{
    CellPossibilities[] EveryCellPossibilities();
    Cell[] EveryCell();
    IPossibilities EveryPossibilities();
}