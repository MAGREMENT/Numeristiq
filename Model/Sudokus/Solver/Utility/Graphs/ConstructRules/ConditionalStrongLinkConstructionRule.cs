using System.Collections.Generic;
using System.Linq;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Coloring;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class ConditionalStrongLinkConstructionRule : IConstructionRule<ISudokuSolverData,
    IConditionalGraph<ISudokuElement, LinkStrength, ElementColor>>
{
    public static ConditionalStrongLinkConstructionRule Instance { get; } = new();
    
    private ConditionalStrongLinkConstructionRule() {}
    
    public int ID { get; } = UniqueConstructionRuleID.Next();
    public void Apply(IConditionalGraph<ISudokuElement, LinkStrength, ElementColor> linkGraph, ISudokuSolverData data)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = data.RowPositionsAt(row, number);
                if(ppir.Count < 3) continue;

                var asArray = ppir.ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        var off = new ISudokuElement[asArray.Length - 2];
                        var cursor = 0;
                        for (int k = 0; k < asArray.Length; k++)
                        {
                            if(k == i || k == j) continue;
                            off[cursor++] = new CellPossibility(row, asArray[k], number);
                        }
                        
                        linkGraph.Add(new ISudokuStrongLinkCondition(off),
                            new CellPossibility(row, asArray[i], number),
                            new CellPossibility(row, asArray[j], number), LinkStrength.Weak);
                    }
                }
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppic = data.ColumnPositionsAt(col, number);
                if(ppic.Count < 3) continue;

                var asArray = ppic.ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        var off = new ISudokuElement[asArray.Length - 2];
                        var cursor = 0;
                        for (int k = 0; k < asArray.Length; k++)
                        {
                            if(k == i || k == j) continue;
                            off[cursor++] = new CellPossibility(asArray[k], col, number);
                        }
                        
                        linkGraph.Add(new ISudokuStrongLinkCondition(off),
                            new CellPossibility(asArray[i], col, number),
                            new CellPossibility(asArray[j], col, number), LinkStrength.Weak);
                    }
                }
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = data.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count < 3) continue;

                    var asArray = ppimn.ToArray();
                    for (int i = 0; i < asArray.Length; i++)
                    {
                        for (int j = i + 1; j < asArray.Length; j++)
                        {
                            var off = new ISudokuElement[asArray.Length - 2];
                            var cursor = 0;
                            for (int k = 0; k < asArray.Length; k++)
                            {
                                if(k == i || k == j) continue;
                                off[cursor++] = new CellPossibility(asArray[k], number);
                            }
                            
                            linkGraph.Add(new ISudokuStrongLinkCondition(off),
                                new CellPossibility(asArray[i], number),
                                new CellPossibility(asArray[j], number), LinkStrength.Weak);
                        }
                    }
                }
            }
        }
        
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
                        var off = new ISudokuElement[asArray.Length - 2];
                        var cursor = 0;
                        for (int k = 0; k < asArray.Length; k++)
                        {
                            if(k == i || k == j) continue;
                            off[cursor++] = new CellPossibility(row, col, asArray[k]);
                        }
                        
                        linkGraph.Add(new ISudokuStrongLinkCondition(off),
                            new CellPossibility(row, col, asArray[i]),
                            new CellPossibility(row, col, asArray[j]), LinkStrength.Weak);
                    }
                }
            }
        }
    }
}

public class ISudokuStrongLinkCondition : ICondition<ISudokuElement, ElementColor>
{
    private readonly IReadOnlyList<ISudokuElement> _off;

    public ISudokuStrongLinkCondition(IReadOnlyList<ISudokuElement> off)
    {
        _off = off;
    }

    public bool IsMet(ValueCollection<ISudokuElement, ElementColor> values)
    {
        foreach (var element in _off)
        {
            if (!values.TryGetValue(element, out var v) || v != ElementColor.Off) return false;
        }

        return true;
    }
}