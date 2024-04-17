using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.StrategiesUtility.AlmostLockedSets;

public class AlmostHiddenSetSearcher
{
    private readonly IStrategyUser _strategyUser;

    public int Max { get; set; } = 5;
    public int Difference { get; set; } = 1;

    public AlmostHiddenSetSearcher(IStrategyUser strategyUser)
    {
        _strategyUser = strategyUser;
    }

    public List<IPossibilitiesPositions> FullGrid()
    {
        List<IPossibilitiesPositions> result = new();

        var poss = new ReadOnlyBitSet16();
        
        var lp = new LinePositions();
        for (int row = 0; row < 9; row++)
        {
            InRow(row, result, 1, lp, poss);
            lp.Void();
            poss = new ReadOnlyBitSet16();
        }

        for (int col = 0; col < 9; col++)
        {
            InColumn(col, result, 1, lp, poss);
            lp.Void();
            poss = new ReadOnlyBitSet16();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                InMiniGrid(miniRow, miniCol, result, 1, 
                    new MiniGridPositions(miniRow, miniCol), poss, true);
                poss = new ReadOnlyBitSet16();
            }
        }

        return result;
    }

    public List<IPossibilitiesPositions> InRow(int row)
    {
        List<IPossibilitiesPositions> result = new();
        
        InRow(row, result, 1, new LinePositions(), new ReadOnlyBitSet16());

        return result;
    }

    private void InRow(int row, 
        List<IPossibilitiesPositions> result, int start, LinePositions current, ReadOnlyBitSet16 possibilities)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = _strategyUser.RowPositionsAt(row, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities += i;

            if (or.Count == possibilities.Count + Difference) result.Add(new CAPPossibilitiesPositions(
                or.ToCellArray(Unit.Row, row), possibilities, _strategyUser));
            
            if (possibilities.Count < Max)
                InRow(row, result, i + 1, or, possibilities);

            possibilities -= i;
        }
    }

    public List<IPossibilitiesPositions> InColumn(int column)
    {
        List<IPossibilitiesPositions> result = new();
        
        InColumn(column, result, 1, new LinePositions(), new ReadOnlyBitSet16());

        return result;
    }
    
    private void InColumn(int column, 
        List<IPossibilitiesPositions> result, int start, LinePositions current, ReadOnlyBitSet16 possibilities)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = _strategyUser.ColumnPositionsAt(column, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities += i;

            if (or.Count == possibilities.Count + Difference) result.Add(new CAPPossibilitiesPositions(
                or.ToCellArray(Unit.Column, column), possibilities, _strategyUser));
            
            if (possibilities.Count < Max)
                InColumn(column, result, i + 1, or, possibilities);

            possibilities -= i;
        }
    }
    
    public List<IPossibilitiesPositions> InMiniGrid(int miniRow, int miniCol)
    {
        List<IPossibilitiesPositions> result = new();
        
        InMiniGrid(miniRow, miniCol, result, 1, new MiniGridPositions(miniRow, miniCol), new ReadOnlyBitSet16());

        return result;
    }
    
    private void InMiniGrid(int miniRow, int miniCol, 
        List<IPossibilitiesPositions> result, int start, MiniGridPositions current, ReadOnlyBitSet16 possibilities,
        bool excludeSameLine = false)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = _strategyUser.MiniGridPositionsAt(miniRow, miniCol, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities += i;

            if (or.Count == possibilities.Count + Difference)
            {
                if(!excludeSameLine || !(or.AreAllInSameColumn() || or.AreAllInSameColumn())) 
                    result.Add(new CAPPossibilitiesPositions(or.ToCellArray(),
                        possibilities, _strategyUser));
            }

            if (possibilities.Count < Max)
                InMiniGrid(miniRow, miniCol, result, i + 1, or, possibilities);

            possibilities -= i;
        }
    }
}