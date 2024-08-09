using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Sudokus.Solver.Utility;

public class BlossomLoopBranch
{
    public ISudokuElement[] Targets { get; }
    public Chain<ISudokuElement, LinkStrength> Branch { get; }

    public BlossomLoopBranch(Chain<ISudokuElement, LinkStrength> branch, params ISudokuElement[] targets)
    {
        Targets = targets;
        Branch = branch;
    }

    public override string ToString()
    {
        return Branch.ToString();
    }
}