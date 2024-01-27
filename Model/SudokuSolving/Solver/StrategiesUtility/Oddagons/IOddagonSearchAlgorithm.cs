using System.Collections.Generic;
using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver.StrategiesUtility.Oddagons;

public interface IOddagonSearchAlgorithm
{
    List<AlmostOddagon> Search(IStrategyManager strategyManager, ILinkGraph<CellPossibility> graph);
}