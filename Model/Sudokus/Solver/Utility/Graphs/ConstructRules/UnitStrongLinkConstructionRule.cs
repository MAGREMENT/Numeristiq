﻿using System.Linq;
using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class UnitStrongLinkConstructionRule : IConstructionRule<ISudokuSolverData, IGraph<ISudokuElement, LinkStrength>>,
    IConstructionRule<ISudokuSolverData, IGraph<CellPossibility, LinkStrength>>//TODO another rule with merged weak & strong links
{
    public static UnitStrongLinkConstructionRule Instance { get; } = new();
    
    private UnitStrongLinkConstructionRule() {}
    
    public int ID { get; } = UniqueConstructionRuleID.Next();
    
    public void Apply(IGraph<ISudokuElement, LinkStrength> linkGraph, ISudokuSolverData data)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = data.RowPositionsAt(row, number);
                if(ppir.Count != 2) continue;

                var asArray = ppir.ToArray();
                linkGraph.Add(new CellPossibility(row, asArray[0], number),
                    new CellPossibility(row, asArray[1], number), LinkStrength.Strong);
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppic = data.ColumnPositionsAt(col, number);
                if(ppic.Count != 2) continue;

                var asArray = ppic.ToArray();
                linkGraph.Add(new CellPossibility(asArray[0], col, number),
                    new CellPossibility(asArray[1], col, number), LinkStrength.Strong);
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = data.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count != 2) continue;

                    var asArray = ppimn.ToArray();
                    linkGraph.Add(new CellPossibility(asArray[0].Row, asArray[0].Column, number),
                        new CellPossibility(asArray[1].Row, asArray[1].Column, number), LinkStrength.Strong);
                }
            }
        }
    }

    public void Apply(IGraph<CellPossibility, LinkStrength> linkGraph, ISudokuSolverData data)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = data.RowPositionsAt(row, number);
                if(ppir.Count != 2) continue;

                var asArray = ppir.ToArray();
                linkGraph.Add(new CellPossibility(row, asArray[0], number),
                    new CellPossibility(row, asArray[1], number), LinkStrength.Strong);
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppic = data.ColumnPositionsAt(col, number);
                if(ppic.Count != 2) continue;

                var asArray = ppic.ToArray();
                linkGraph.Add(new CellPossibility(asArray[0], col, number),
                    new CellPossibility(asArray[1], col, number), LinkStrength.Strong);
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = data.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count != 2) continue;

                    var asArray = ppimn.ToArray();
                    linkGraph.Add(new CellPossibility(asArray[0].Row, asArray[0].Column, number),
                        new CellPossibility(asArray[1].Row, asArray[1].Column, number), LinkStrength.Strong);
                }
            }
        }
    }
}