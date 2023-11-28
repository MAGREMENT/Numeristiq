using System.Linq;
using Global.Enums;

namespace Model.Solver.StrategiesUtility.Graphs.ConstructRules;

public class XYChainSpecificConstructRule : IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager)
    {
        
    }

    public void Apply(LinkGraph<CellPossibility> linkGraph, IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.RowPositionsAt(row, number);
                if(ppir.Count < 2) continue;

                var asArray = ppir.ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    if (strategyManager.PossibilitiesAt(row, asArray[i]).Count != 2) continue;
                    
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        if (strategyManager.PossibilitiesAt(row, asArray[j]).Count != 2) continue;
                        
                        linkGraph.AddLink(new CellPossibility(row, asArray[i], number),
                            new CellPossibility(row, asArray[j], number), LinkStrength.Weak);
                    }
                }
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.ColumnPositionsAt(col, number);
                if(ppic.Count < 2) continue;

                var asArray = ppic.ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    if (strategyManager.PossibilitiesAt(asArray[i], col).Count != 2) continue;
                    
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        if (strategyManager.PossibilitiesAt(asArray[j], col).Count != 2) continue;
                        
                        linkGraph.AddLink(new CellPossibility(asArray[i], col, number),
                            new CellPossibility(asArray[j], col, number), LinkStrength.Weak);
                    }
                }
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = strategyManager.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count < 2) continue;

                    var asArray = ppimn.ToArray();
                    for (int i = 0; i < asArray.Length; i++)
                    {
                        if (strategyManager.PossibilitiesAt(asArray[i]).Count != 2) continue;
                        
                        for (int j = i + 1; j < asArray.Length; j++)
                        {
                            if (strategyManager.PossibilitiesAt(asArray[j]).Count != 2) continue;
                            
                            linkGraph.AddLink(new CellPossibility(asArray[i].Row, asArray[i].Col, number),
                                new CellPossibility(asArray[j].Row, asArray[j].Col, number), LinkStrength.Weak);
                        }
                    }
                }
            }
        }
    }
}