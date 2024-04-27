using Model.Helpers.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Tectonics.Utility.ConstructRules;

public class CellLinkConstructRule : IConstructRule<ITectonicStrategyUser, ITectonicElement>
{
    public void Apply(ILinkGraph<ITectonicElement> linkGraph, ITectonicStrategyUser strategyUser)
    {
        for (int row = 0; row < strategyUser.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < strategyUser.Tectonic.ColumnCount; col++)
            {
                var poss = strategyUser.PossibilitiesAt(row, col);
                if (poss.Count == 2)
                {
                    var a = 0;
                    poss.Next(ref a);
                    var first = new CellPossibility(row, col, a);
                    poss.Next(ref a);
                    linkGraph.Add(first, new CellPossibility(row, col, a), LinkStrength.Strong);
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

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ITectonicStrategyUser strategyUser)
    {
        for (int row = 0; row < strategyUser.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < strategyUser.Tectonic.ColumnCount; col++)
            {
                var poss = strategyUser.PossibilitiesAt(row, col);
                if (poss.Count == 2)
                {
                    var a = 0;
                    poss.Next(ref a);
                    var first = new CellPossibility(row, col, a);
                    poss.Next(ref a);
                    linkGraph.Add(first, new CellPossibility(row, col, a), LinkStrength.Strong);
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