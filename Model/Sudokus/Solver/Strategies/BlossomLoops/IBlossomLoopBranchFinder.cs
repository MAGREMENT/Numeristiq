using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopBranchFinder
{
    BlossomLoopBranch[]? FindShortestBranches(IGraph<ISudokuElement, LinkStrength> graph,
        CellPossibility[] cps, Loop<ISudokuElement, LinkStrength> loop);
}