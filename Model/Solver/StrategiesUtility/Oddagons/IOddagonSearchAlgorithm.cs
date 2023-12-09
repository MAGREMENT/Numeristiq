using System.Collections.Generic;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.Oddagons;

public interface IOddagonSearchAlgorithm
{
    List<AlmostOddagon> Search(IStrategyManager strategyManager, LinkGraph<CellPossibility> graph);
}