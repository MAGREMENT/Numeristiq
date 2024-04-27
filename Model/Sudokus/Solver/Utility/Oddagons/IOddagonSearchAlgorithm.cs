using System.Collections.Generic;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Oddagons;

public interface IOddagonSearchAlgorithm
{
    List<AlmostOddagon> Search(ISudokuStrategyUser strategyUser, ILinkGraph<CellPossibility> graph);
}