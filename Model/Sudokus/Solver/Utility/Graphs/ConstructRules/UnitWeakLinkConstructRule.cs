using System.Linq;
using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class UnitWeakLinkConstructRule : IConstructRule<ISudokuSolverData, ISudokuElement>,
    IConstructRule<ISudokuSolverData, CellPossibility>
{
    public static UnitWeakLinkConstructRule Instance { get; } = new();
    
    private UnitWeakLinkConstructRule() {}
    
    public int ID { get; } = UniqueConstructRuleID.Next();
    
    public void Apply(IGraph<ISudokuElement, LinkStrength> linkGraph, ISudokuSolverData data)
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
                        linkGraph.Add(new CellPossibility(row, asArray[i], number),
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
                        linkGraph.Add(new CellPossibility(asArray[i], col, number),
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
                            linkGraph.Add(new CellPossibility(asArray[i], number),
                                new CellPossibility(asArray[j], number), LinkStrength.Weak);
                        }
                    }
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
                if(ppir.Count < 3) continue;

                var asArray = ppir.ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.Add(new CellPossibility(row, asArray[i], number),
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
                        linkGraph.Add(new CellPossibility(asArray[i], col, number),
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
                            linkGraph.Add(new CellPossibility(asArray[i].Row, asArray[i].Column, number),
                                new CellPossibility(asArray[j].Row, asArray[j].Column, number), LinkStrength.Weak);
                        }
                    }
                }
            }
        }
    }
}