using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class JuniorExocetConstructRule : IConstructRule<ISudokuSolverData, ISudokuElement> //TODO
{
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, ISudokuSolverData solverData)
    {
        
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ISudokuSolverData solverData)
    {
        
    }
}