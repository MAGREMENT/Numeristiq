using System.Collections.Generic;
using Model.SudokuSolving.Solver.StrategiesUtility;

namespace Model.SudokuSolving.Solver.Strategies.BlossomLoops.Types;

public class UnitType : IBlossomLoopType
{
    public string Name => BlossomLoopStrategy.OfficialNameForUnit;
    public IEnumerable<CellPossibility[]> Candidates(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var lp = strategyManager.RowPositionsAt(row, number);
                if (lp.Count < 3) continue;
                
                var cps = new CellPossibility[lp.Count];
                int cursor = 0;
                foreach (var col in lp)
                {
                    cps[cursor++] = new CellPossibility(row, col, number);
                }

                yield return cps;
            }
            
            for (int col = 0; col < 9; col++)
            {
                var lp = strategyManager.ColumnPositionsAt(col, number);
                if (lp.Count < 3) continue;
                
                var cps = new CellPossibility[lp.Count];
                int cursor = 0;
                foreach (var row in lp)
                {
                    cps[cursor++] = new CellPossibility(row, col, number);
                }

                yield return cps;
            }

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var mgp = strategyManager.MiniGridPositionsAt(r, c, number);
                    if (mgp.Count < 3) continue;
                    
                    var cps = new CellPossibility[mgp.Count];
                    int cursor = 0;
                    foreach (var cell in mgp)
                    {
                        cps[cursor++] = new CellPossibility(cell, number);
                    }

                    yield return cps;
                }
            }
        }
    }
}