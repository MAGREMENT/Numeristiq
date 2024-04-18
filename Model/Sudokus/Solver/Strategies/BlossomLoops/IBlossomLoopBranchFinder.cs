using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopBranchFinder
{
    BlossomLoopBranch[]? FindShortestBranches(ILinkGraph<ISudokuElement> graph,
        CellPossibility[] cps, LinkGraphLoop<ISudokuElement> loop);
}