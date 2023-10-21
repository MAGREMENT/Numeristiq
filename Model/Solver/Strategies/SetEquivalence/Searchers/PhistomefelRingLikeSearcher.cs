using System.Collections.Generic;
using Model.Solver.Positions;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies.SetEquivalence.Searchers;

public class PhistomefelRingLikeSearcher : ISetEquivalenceSearcher
{
    private const int HouseCount = 4;
    
    public IEnumerable<SetEquivalence> Search(IStrategyManager strategyManager)
    {
        List<SetEquivalence> result = new();
        GridPositions gp1 = new();
        GridPositions gp2 = new();

        for (int firstRow = 0; firstRow < 9; firstRow++)
        {
            gp1.FillRow(firstRow);
            int miniRow = firstRow / 3;

            for (int miniCol = 0; miniCol < 9; miniCol++)
            {
                gp2.FillMiniGrid(miniRow, miniCol);

                ContinueSearchColumn(result, gp1, gp2, miniCol, 1);

                gp2.VoidMiniGrid(miniRow, miniCol);
            }
            
            gp2.VoidRow(firstRow);
        }

        return result;
    }

    private void ContinueSearchColumn(List<SetEquivalence> result, GridPositions gp1, GridPositions gp2,
        int lastMiniCol, int count)
    {
        var startCol = lastMiniCol * 3;

        for (int n = 0; n < 3; n++)
        {
            var col = startCol + n;

            gp1.FillColumn(col);
            int miniCol = col / 3;
            
            for(int miniRow = 0; miniRow < 3; miniRow++)
            {
                gp2.FillMiniGrid(miniRow, miniCol);
                
                if (count == HouseCount) Add(result, gp1, gp2);
                else ContinueSearchRow(result, gp1, gp2, miniRow, count + 1);

                gp2.VoidMiniGrid(miniRow, miniCol);
            }

            gp1.VoidColumn(col);
        }
    }
    
    private void ContinueSearchRow(List<SetEquivalence> result, GridPositions gp1, GridPositions gp2,
        int lastMiniRow, int count)
    {
        var startRow = lastMiniRow * 3;

        for (int n = 0; n < 3; n++)
        {
            var row = startRow + n;

            gp1.FillRow(row);
            int miniRow = row / 3;
            
            for(int miniCol = 0; miniCol < 3; miniCol++)
            {
                gp2.FillMiniGrid(miniRow, miniCol);

                if (count == HouseCount) Add(result, gp1, gp2);
                else ContinueSearchColumn(result, gp1, gp2, miniCol, count + 1);

                gp2.VoidMiniGrid(miniRow, miniCol);
            }

            gp1.VoidRow(row);
        }
    }

    private void Add(List<SetEquivalence> result, GridPositions gp1, GridPositions gp2)
    {
        var toAdd = new SetEquivalence(new List<Cell>(gp1.Difference(gp2)), HouseCount,
            new List<Cell>(gp2.Difference(gp1)), HouseCount);
        result.Add(toAdd);
    }
}