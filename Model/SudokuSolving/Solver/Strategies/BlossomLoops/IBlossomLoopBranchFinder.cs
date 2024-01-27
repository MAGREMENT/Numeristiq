using Model.SudokuSolving.Solver.StrategiesUtility;
using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopBranchFinder
{
    BlossomLoopBranch[]? FindShortestBranches(ILinkGraph<ISudokuElement> graph,
        CellPossibility[] cps, LinkGraphLoop<ISudokuElement> loop);
}