using System.Collections.Generic;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;

namespace Model.Sudokus.Solver.StrategiesUtility.Oddagons;

public interface IOddagonSearchAlgorithm
{
    List<AlmostOddagon> Search(IStrategyUser strategyUser, ILinkGraph<CellPossibility> graph);
}