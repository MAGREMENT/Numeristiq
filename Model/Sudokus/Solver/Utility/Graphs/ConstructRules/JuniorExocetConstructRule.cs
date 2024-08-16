using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class JuniorExocetConstructRule : IConstructRule<ISudokuSolverData, ISudokuElement> //TODO
{
    public static JuniorExocetConstructRule Instance { get; } = new();
    
    private JuniorExocetConstructRule(){}
    
    public int ID { get; } = UniqueConstructRuleID.Next();
    
    public void Apply(IGraph<ISudokuElement, LinkStrength> linkGraph, ISudokuSolverData data)
    {
        
    }
}