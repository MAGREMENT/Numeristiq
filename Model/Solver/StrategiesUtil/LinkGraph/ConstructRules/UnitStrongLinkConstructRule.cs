﻿using System.Linq;

namespace Model.Solver.StrategiesUtil.LinkGraph.ConstructRules;

public class UnitStrongLinkConstructRule : IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.RowPositionsAt(row, number);
                if(ppir.Count != 2) continue;

                var asArray = ppir.ToArray();
                linkGraph.AddLink(new CellPossibility(row, asArray[0], number),
                    new CellPossibility(row, asArray[1], number), LinkStrength.Strong);
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.ColumnPositionsAt(col, number);
                if(ppic.Count != 2) continue;

                var asArray = ppic.ToArray();
                linkGraph.AddLink(new CellPossibility(asArray[0], col, number),
                    new CellPossibility(asArray[1], col, number), LinkStrength.Strong);
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = strategyManager.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count != 2) continue;

                    var asArray = ppimn.ToArray();
                    linkGraph.AddLink(new CellPossibility(asArray[0].Row, asArray[0].Col, number),
                        new CellPossibility(asArray[1].Row, asArray[1].Col, number), LinkStrength.Strong);
                }
            }
        }
    }
}