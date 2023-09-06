using System.Linq;
using Model.Solver;

namespace Model.StrategiesUtil.LinkGraph.ConstructRules;

public class UnitWeakLinkConstructRule : IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.RowPositions(row, number);
                if(ppir.Count < 3) continue;

                var asArray = ppir.ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.AddLink(new PossibilityCoordinate(row, asArray[i], number),
                            new PossibilityCoordinate(row, asArray[j], number), LinkStrength.Weak);
                    }
                }
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.ColumnPositions(col, number);
                if(ppic.Count < 3) continue;

                var asArray = ppic.ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.AddLink(new PossibilityCoordinate(asArray[i], col, number),
                            new PossibilityCoordinate(asArray[j], col, number), LinkStrength.Weak);
                    }
                }
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = strategyManager.MiniGridPositions(miniRow, miniCol, number);
                    if (ppimn.Count < 3) continue;

                    var asArray = ppimn.ToArray();
                    for (int i = 0; i < asArray.Length; i++)
                    {
                        for (int j = i + 1; j < asArray.Length; j++)
                        {
                            linkGraph.AddLink(new PossibilityCoordinate(asArray[i].Row, asArray[i].Col, number),
                                new PossibilityCoordinate(asArray[j].Row, asArray[j].Col, number), LinkStrength.Weak);
                        }
                    }
                }
            }
        }
    }
}