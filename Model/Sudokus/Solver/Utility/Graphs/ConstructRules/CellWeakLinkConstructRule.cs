using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class CellWeakLinkConstructRule : IConstructRule<ISudokuSolverData, ISudokuElement>,
    IConstructRule<ISudokuSolverData, CellPossibility>
{
    public static CellWeakLinkConstructRule Instance { get; } = new();
    
    private CellWeakLinkConstructRule() {}
    
    public int ID { get; } = UniqueConstructRuleID.Next();
    
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