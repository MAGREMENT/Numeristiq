using Model.Solver.Possibility;

namespace Model.Solver.StrategiesUtil.LinkGraph;

public interface ILinkGraphElement
{
    CellPossibilities[] EveryCellPossibilities();
    Cell[] EveryCell();
    Possibilities EveryPossibilities();
}