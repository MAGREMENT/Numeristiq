using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class JuniorExocetConstructionRule : IConstructionRule<ISudokuSolverData, IGraph<ISudokuElement, LinkStrength>> //TODO
{
    public static JuniorExocetConstructionRule Instance { get; } = new();
    
    private JuniorExocetConstructionRule(){}
    
    public int ID { get; } = UniqueConstructionRuleID.Next();
    
    public void Apply(IGraph<ISudokuElement, LinkStrength> linkGraph, ISudokuSolverData data)
    {
        
    }
}