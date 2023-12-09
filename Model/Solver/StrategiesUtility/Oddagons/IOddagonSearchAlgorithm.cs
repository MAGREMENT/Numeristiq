using System.Collections.Generic;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.Oddagons;

public interface IOddagonSearchAlgorithm
{
    List<LinkGraphLoop<CellPossibility>> Search(IStrategyManager strategyManager);
}