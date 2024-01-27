using System.Collections.Generic;
using Model.SudokuSolving.Solver.StrategiesUtility;
using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopLoopFinder
{
    List<LinkGraphLoop<ISudokuElement>> Find(CellPossibility[] cps, ILinkGraph<ISudokuElement> graph);
}