using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver.StrategiesUtility;

public class BlossomLoopBranch
{
    public ISudokuElement[] Targets { get; }
    public LinkGraphChain<ISudokuElement> Branch { get; }

    public BlossomLoopBranch(LinkGraphChain<ISudokuElement> branch, params ISudokuElement[] targets)
    {
        Targets = targets;
        Branch = branch;
    }

    public override string ToString()
    {
        return Branch.ToString();
    }
}