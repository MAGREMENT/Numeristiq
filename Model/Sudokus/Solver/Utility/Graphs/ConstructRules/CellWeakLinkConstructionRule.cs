using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class CellWeakLinkConstructionRule : IConstructionRule<ISudokuSolverData, IGraph<ISudokuElement, LinkStrength>>,
    IConstructionRule<ISudokuSolverData, IGraph<CellPossibility, LinkStrength>>
{
    public static CellWeakLinkConstructionRule Instance { get; } = new();
    
    private CellWeakLinkConstructionRule() {}
    
    public int ID { get; } = UniqueConstructionRuleID.Next();
    
    public void Apply(IGraph<ISudokuElement, LinkStrength> linkGraph, ISudokuSolverData data)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(data.PossibilitiesAt(row, col).Count < 3) continue;

                var asArray = data.PossibilitiesAt(row, col).ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.Add(new CellPossibility(row, col, asArray[i]),
                            new CellPossibility(row, col, asArray[j]), LinkStrength.Weak);
                    }
                }
            }
        }
    }

    public void Apply(IGraph<CellPossibility, LinkStrength> linkGraph, ISudokuSolverData data)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(data.PossibilitiesAt(row, col).Count < 3) continue;

                var asArray = data.PossibilitiesAt(row, col).ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.Add(new CellPossibility(row, col, asArray[i]),
                            new CellPossibility(row, col, asArray[j]), LinkStrength.Weak);
                    }
                }
            }
        }
    }
}