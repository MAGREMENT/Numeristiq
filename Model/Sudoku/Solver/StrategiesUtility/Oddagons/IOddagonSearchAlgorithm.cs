using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.StrategiesUtility.Oddagons;

public interface IOddagonSearchAlgorithm
{
    List<AlmostOddagon> Search(IStrategyManager strategyManager, ILinkGraph<CellPossibility> graph);
}