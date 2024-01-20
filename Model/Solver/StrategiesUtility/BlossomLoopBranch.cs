using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility;

public class BlossomLoopBranch
{
    public IChainingElement[] Targets { get; }
    public LinkGraphChain<IChainingElement> Branch { get; }

    public BlossomLoopBranch(LinkGraphChain<IChainingElement> branch, params IChainingElement[] targets)
    {
        Targets = targets;
        Branch = branch;
    }

    public override string ToString()
    {
        return Branch.ToString();
    }
}