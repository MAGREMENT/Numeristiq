using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Solver.Utility.ConstructRules;

public class CellLinkConstructRule : IConstructRule<ITectonicSolverData, ITectonicElement>
{
    public static CellLinkConstructRule Instance { get; } = new();
    
    private CellLinkConstructRule() {}
    
    public int ID { get; } = UniqueConstructRuleID.Next();
    
    public void Apply(IGraph<ITectonicElement, LinkStrength> linkGraph, ITectonicSolverData data)
    {
        for (int row = 0; row < data.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < data.Tectonic.ColumnCount; col++)
            {
                var poss = data.PossibilitiesAt(row, col);
                if (poss.Count == 2)
                {
                    var a = poss.NextPossibility(0);
                    linkGraph.Add(new CellPossibility(row, col, a),
                        new CellPossibility(row, col, poss.NextPossibility(a)), LinkStrength.Strong);
                }
                else if (poss.Count > 2)
                {
                    var asArray = poss.ToArray();
                    for (int i = 0; i < asArray.Length - 1; i++)
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
}