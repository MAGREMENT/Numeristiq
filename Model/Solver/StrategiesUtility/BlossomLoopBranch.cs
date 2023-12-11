using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility;

public class BlossomLoopBranch
{
    public ILinkGraphElement[] Targets { get; }
    public LinkGraphChain<ILinkGraphElement> Branch { get; }

    public BlossomLoopBranch(LinkGraphChain<ILinkGraphElement> branch, params ILinkGraphElement[] targets)
    {
        Targets = targets;
        Branch = branch;
    }

    public override string ToString()
    {
        return Branch.ToString();
    }
}