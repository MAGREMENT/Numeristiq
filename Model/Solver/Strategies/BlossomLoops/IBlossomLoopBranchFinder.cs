using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopBranchFinder
{
    BlossomLoopBranch[]? FindShortestBranches(LinkGraph<ILinkGraphElement> graph,
        CellPossibility[] cps, LinkGraphLoop<ILinkGraphElement> loop);
}