using System.Collections.Generic;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopLoopFinder
{
    List<LinkGraphLoop<ISudokuElement>> Find(CellPossibility[] cps, ILinkGraph<ISudokuElement> graph);
}