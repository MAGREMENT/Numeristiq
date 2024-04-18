using System.Collections.Generic;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Sudokus.Solver.Utility.Oddagons;

public interface IOddagonSearchAlgorithm
{
    List<AlmostOddagon> Search(IStrategyUser strategyUser, ILinkGraph<CellPossibility> graph);
}