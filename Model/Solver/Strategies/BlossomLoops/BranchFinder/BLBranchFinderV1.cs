using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops.BranchFinder;

public class BLBranchFinderV1 : IBlossomLoopBranchFinder
{
    public LinkGraphChain<ILinkGraphElement> FindShortestBranch(LinkGraph<ILinkGraphElement> graph, CellPossibility[] cps, LinkGraphLoop<ILinkGraphElement> loop)
    {
        throw new System.NotImplementedException();
    }
}