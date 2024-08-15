using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class JuniorExocetConstructRule : IConstructRule<ISudokuSolverData, ISudokuElement> //TODO
{
    public static JuniorExocetConstructRule Instance { get; } = new();
    
    private JuniorExocetConstructRule(){}
    
    public int ID { get; } = UniqueID.Next();
    
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, ISudokuSolverData solverData)
    {
        
    }
}