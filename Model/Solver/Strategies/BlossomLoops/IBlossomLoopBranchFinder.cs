using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopBranchFinder
{
    LinkGraphChain<ILinkGraphElement> FindShortestBranch(LinkGraph<ILinkGraphElement> graph,
        CellPossibility[] cps, LinkGraphLoop<ILinkGraphElement> loop);
}