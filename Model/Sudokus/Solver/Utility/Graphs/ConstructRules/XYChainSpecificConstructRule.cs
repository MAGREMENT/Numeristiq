using System.Linq;
using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class XYChainSpecificConstructRule : IConstructRule<ISudokuSolverData, ISudokuElement>
{
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, ISudokuSolverData solverData)
    {
        
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ISudokuSolverData solverData)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = solverData.RowPositionsAt(row, number);
                if(ppir.Count < 2) continue;

                var asArray = ppir.ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    if (solverData.PossibilitiesAt(row, asArray[i]).Count != 2) continue;
                    
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        if (solverData.PossibilitiesAt(row, asArray[j]).Count != 2) continue;
                        
                        linkGraph.Add(new CellPossibility(row, asArray[i], number),
                            new CellPossibility(row, asArray[j], number), LinkStrength.Weak);
                    }
                }
            }
            
            for (int col = 0; col < 9; col++)
            {
                var ppic = solverData.ColumnPositionsAt(col, number);
                if(ppic.Count < 2) continue;

                var asArray = ppic.ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    if (solverData.PossibilitiesAt(asArray[i], col).Count != 2) continue;
                    
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        if (solverData.PossibilitiesAt(asArray[j], col).Count != 2) continue;
                        
                        linkGraph.Add(new CellPossibility(asArray[i], col, number),
                            new CellPossibility(asArray[j], col, number), LinkStrength.Weak);
                    }
                }
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = solverData.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count < 2) continue;

                    var asArray = ppimn.ToArray();
                    for (int i = 0; i < asArray.Length; i++)
                    {
                        if (solverData.PossibilitiesAt(asArray[i]).Count != 2) continue;
                        
                        for (int j = i + 1; j < asArray.Length; j++)
                        {
                            if (solverData.PossibilitiesAt(asArray[j]).Count != 2) continue;
                            
                            linkGraph.Add(new CellPossibility(asArray[i].Row, asArray[i].Column, number),
                                new CellPossibility(asArray[j].Row, asArray[j].Column, number), LinkStrength.Weak);
                        }
                    }
                }
            }
        }
    }
}