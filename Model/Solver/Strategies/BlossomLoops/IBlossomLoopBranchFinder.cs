using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopBranchFinder
{
    BlossomLoopBranch[]? FindShortestBranches(ILinkGraph<ISudokuElement> graph,
        CellPossibility[] cps, LinkGraphLoop<ISudokuElement> loop);
}