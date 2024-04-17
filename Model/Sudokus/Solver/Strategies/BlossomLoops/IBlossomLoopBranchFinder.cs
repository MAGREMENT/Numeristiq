using Model.Sudokus.Solver.StrategiesUtility;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopBranchFinder
{
    BlossomLoopBranch[]? FindShortestBranches(ILinkGraph<ISudokuElement> graph,
        CellPossibility[] cps, LinkGraphLoop<ISudokuElement> loop);
}