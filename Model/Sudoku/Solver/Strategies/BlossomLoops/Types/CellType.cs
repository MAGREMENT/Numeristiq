using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Sudoku.Solver.Strategies.BlossomLoops.Types;

public class CellType : IBlossomLoopType
{
    public string Name => BlossomLoopStrategy.OfficialNameForCell;
    public IEnumerable<CellPossibility[]> Candidates(IStrategyUser strategyUser)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var poss = strategyUser.PossibilitiesAt(row, col);
                if (poss.Count < 3) continue;

                var cps = new CellPossibility[poss.Count];
                int cursor = 0;
                foreach (var p in poss.EnumeratePossibilities())
                {
                    cps[cursor++] = new CellPossibility(row, col, p);
                }

                yield return cps;
            }
        }
    }
}