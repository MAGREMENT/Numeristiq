using System.Collections.Generic;
using Model.Sudokus.Solver.StrategiesUtility;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopLoopFinder
{
    List<LinkGraphLoop<ISudokuElement>> Find(CellPossibility[] cps, ILinkGraph<ISudokuElement> graph);
}