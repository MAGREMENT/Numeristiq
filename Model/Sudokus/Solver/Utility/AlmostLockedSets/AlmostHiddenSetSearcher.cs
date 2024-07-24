using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.AlmostLockedSets;

public class AlmostHiddenSetSearcher
{
    private readonly ISudokuSolverData _solverData;
    private int _maxSize = 5;
    private int _difference = 1;

    public AlmostHiddenSetSearcher(ISudokuSolverData solverData)
    {
        _solverData = solverData;
    }

    public List<IPossibilitySet> FullGrid(int maxSize, int difference)
    {
        List<IPossibilitySet> result = new();
        var poss = new ReadOnlyBitSet16();
        _maxSize = maxSize;
        _difference = difference;
        
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
                    new BoxPositions(miniRow, miniCol), poss, true);
                poss = new ReadOnlyBitSet16();
            }
        }

        return result;
    }

    public List<IPossibilitySet> InRow(int row, int maxSize, int difference)
    {
        List<IPossibilitySet> result = new();
        
        _maxSize = maxSize;
        _difference = difference;
        InRow(row, result, 1, new LinePositions(), new ReadOnlyBitSet16());

        return result;
    }

    public List<IPossibilitySet> InColumn(int column, int maxSize, int difference)
    {
        List<IPossibilitySet> result = new();
        
        _maxSize = maxSize;
        _difference = difference;
        InColumn(column, result, 1, new LinePositions(), new ReadOnlyBitSet16());

        return result;
    }
    
    public List<IPossibilitySet> InMiniGrid(int miniRow, int miniCol, int maxSize, int difference)
    {
        List<IPossibilitySet> result = new();
        
        _maxSize = maxSize;
        _difference = difference;
        InMiniGrid(miniRow, miniCol, result, 1, new BoxPositions(miniRow, miniCol), new ReadOnlyBitSet16());

        return result;
    }
    
    private void InRow(int row, 
        List<IPossibilitySet> result, int start, LinePositions current, ReadOnlyBitSet16 possibilities)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = _solverData.RowPositionsAt(row, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities += i;

            if (or.Count == possibilities.Count + _difference) result.Add(new SnapshotPossibilitySet(
                or.ToCellArray(Unit.Row, row), possibilities, _solverData.CurrentState));
            
            if (possibilities.Count < _maxSize)
                InRow(row, result, i + 1, or, possibilities);

            possibilities -= i;
        }
    }
    
    private void InColumn(int column, 
        List<IPossibilitySet> result, int start, LinePositions current, ReadOnlyBitSet16 possibilities)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = _solverData.ColumnPositionsAt(column, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities += i;

            if (or.Count == possibilities.Count + _difference) result.Add(new SnapshotPossibilitySet(
                or.ToCellArray(Unit.Column, column), possibilities, _solverData.CurrentState));
            
            if (possibilities.Count < _maxSize)
                InColumn(column, result, i + 1, or, possibilities);

            possibilities -= i;
        }
    }
    
    private void InMiniGrid(int miniRow, int miniCol, 
        List<IPossibilitySet> result, int start, BoxPositions current, ReadOnlyBitSet16 possibilities,
        bool excludeSameLine = false)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = _solverData.MiniGridPositionsAt(miniRow, miniCol, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities += i;

            if (or.Count == possibilities.Count + _difference)
            {
                if(!excludeSameLine || !(or.AreAllInSameColumn() || or.AreAllInSameColumn())) 
                    result.Add(new SnapshotPossibilitySet(or.ToCellArray(),
                        possibilities, _solverData.CurrentState));
            }

            if (possibilities.Count < _maxSize)
                InMiniGrid(miniRow, miniCol, result, i + 1, or, possibilities);

            possibilities -= i;
        }
    }
}