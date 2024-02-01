using System.Collections.Generic;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Solver.PossibilityPosition;

namespace Model.Sudoku.Solver.StrategiesUtility.AlmostLockedSets;

public class AlmostHiddenSetSearcher
{
    private readonly IStrategyManager _strategyManager;

    public int Max { get; set; } = 5;
    public int Difference { get; set; } = 1;

    public AlmostHiddenSetSearcher(IStrategyManager strategyManager)
    {
        _strategyManager = strategyManager;
    }

    public List<IPossibilitiesPositions> FullGrid()
    {
        List<IPossibilitiesPositions> result = new();

        var poss = Possibilities.NewEmpty();
        
        var lp = new LinePositions();
        for (int row = 0; row < 9; row++)
        {
            InRow(row, result, 1, lp, poss);
            lp.Void();
            poss.RemoveAll();
        }

        for (int col = 0; col < 9; col++)
        {
            InColumn(col, result, 1, lp, poss);
            lp.Void();
            poss.RemoveAll();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                InMiniGrid(miniRow, miniCol, result, 1, 
                    new MiniGridPositions(miniRow, miniCol), poss, true);
                poss.RemoveAll();
            }
        }

        return result;
    }

    public List<IPossibilitiesPositions> InRow(int row)
    {
        List<IPossibilitiesPositions> result = new();
        
        InRow(row, result, 1, new LinePositions(), Possibilities.NewEmpty());

        return result;
    }

    private void InRow(int row, 
        List<IPossibilitiesPositions> result, int start, LinePositions current, Possibilities possibilities)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = _strategyManager.RowPositionsAt(row, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities.Add(i);

            if (or.Count == possibilities.Count + Difference) result.Add(new CAPPossibilitiesPositions(
                or.ToCellArray(Unit.Row, row), possibilities.Copy(), _strategyManager));
            
            if (possibilities.Count < Max)
                InRow(row, result, i + 1, or, possibilities);

            possibilities.Remove(i);
        }
    }

    public List<IPossibilitiesPositions> InColumn(int column)
    {
        List<IPossibilitiesPositions> result = new();
        
        InColumn(column, result, 1, new LinePositions(), Possibilities.NewEmpty());

        return result;
    }
    
    private void InColumn(int column, 
        List<IPossibilitiesPositions> result, int start, LinePositions current, Possibilities possibilities)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = _strategyManager.ColumnPositionsAt(column, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities.Add(i);

            if (or.Count == possibilities.Count + Difference) result.Add(new CAPPossibilitiesPositions(
                or.ToCellArray(Unit.Column, column), possibilities.Copy(), _strategyManager));
            
            if (possibilities.Count < Max)
                InColumn(column, result, i + 1, or, possibilities);

            possibilities.Remove(i);
        }
    }
    
    public List<IPossibilitiesPositions> InMiniGrid(int miniRow, int miniCol)
    {
        List<IPossibilitiesPositions> result = new();
        
        InMiniGrid(miniRow, miniCol, result, 1, new MiniGridPositions(miniRow, miniCol), Possibilities.NewEmpty());

        return result;
    }
    
    private void InMiniGrid(int miniRow, int miniCol, 
        List<IPossibilitiesPositions> result, int start, MiniGridPositions current, Possibilities possibilities,
        bool excludeSameLine = false)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = _strategyManager.MiniGridPositionsAt(miniRow, miniCol, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities.Add(i);

            if (or.Count == possibilities.Count + Difference)
            {
                if(!excludeSameLine || !(or.AreAllInSameColumn() || or.AreAllInSameColumn())) 
                    result.Add(new CAPPossibilitiesPositions(or.ToCellArray(),
                        possibilities.Copy(), _strategyManager));
            }

            if (possibilities.Count < Max)
                InMiniGrid(miniRow, miniCol, result, i + 1, or, possibilities);

            possibilities.Remove(i);
        }
    }
}