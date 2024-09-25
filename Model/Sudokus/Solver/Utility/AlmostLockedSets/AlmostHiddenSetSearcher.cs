using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.AlmostLockedSets;

public static class AlmostHiddenSetSearcher
{
    public static List<IPossibilitySet> FullGrid(ISudokuSolverData solverData, int maxSize, int difference)
    {
        List<IPossibilitySet> result = new();
        var poss = new ReadOnlyBitSet16();
        
        var lp = new LinePositions();
        for (int row = 0; row < 9; row++)
        {
            InRow(solverData, maxSize, difference, row, result, 1, lp, poss);
            lp.Void();
            poss = new ReadOnlyBitSet16();
        }

        for (int col = 0; col < 9; col++)
        {
            InColumn(solverData, maxSize, difference, col, result, 1, lp, poss);
            lp.Void();
            poss = new ReadOnlyBitSet16();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                InMiniGrid(solverData, maxSize, difference, miniRow, miniCol, result, 1, 
                    new BoxPositions(miniRow, miniCol), poss, true);
                poss = new ReadOnlyBitSet16();
            }
        }

        return result;
    }

    public static List<IPossibilitySet> InRow(ISudokuSolverData solverData, int maxSize, int difference, int row)
    {
        List<IPossibilitySet> result = new();
        InRow(solverData, maxSize, difference, row, result, 1, new LinePositions(), new ReadOnlyBitSet16());

        return result;
    }

    public static List<IPossibilitySet> InColumn(ISudokuSolverData solverData, int maxSize, int difference, int column)
    {
        List<IPossibilitySet> result = new();
        InColumn(solverData, maxSize, difference, column, result, 1, new LinePositions(), new ReadOnlyBitSet16());

        return result;
    }
    
    public static List<IPossibilitySet> InMiniGrid(ISudokuSolverData solverData, int maxSize, int difference, int miniRow, int miniCol)
    {
        List<IPossibilitySet> result = new();
        InMiniGrid(solverData, maxSize, difference, miniRow, miniCol, result, 1, new BoxPositions(miniRow, miniCol), new ReadOnlyBitSet16());

        return result;
    }
    
    private static void InRow(ISudokuSolverData solverData, int maxSize, int difference, int row, 
        List<IPossibilitySet> result, int start, LinePositions current, ReadOnlyBitSet16 possibilities)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = solverData.RowPositionsAt(row, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities += i;

            if (or.Count == possibilities.Count + difference) result.Add(new SnapshotPossibilitySet(
                or.ToCellArray(Unit.Row, row), possibilities, solverData.CurrentState));
            
            if (possibilities.Count < maxSize)
                InRow(solverData, maxSize, difference, row, result, i + 1, or, possibilities);

            possibilities -= i;
        }
    }
    
    private static void InColumn(ISudokuSolverData solverData, int maxSize, int difference, int column, 
        List<IPossibilitySet> result, int start, LinePositions current, ReadOnlyBitSet16 possibilities)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = solverData.ColumnPositionsAt(column, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities += i;

            if (or.Count == possibilities.Count + difference) result.Add(new SnapshotPossibilitySet(
                or.ToCellArray(Unit.Column, column), possibilities, solverData.CurrentState));
            
            if (possibilities.Count < maxSize)
                InColumn(solverData, maxSize, difference, column, result, i + 1, or, possibilities);

            possibilities -= i;
        }
    }
    
    private static void InMiniGrid(ISudokuSolverData solverData, int maxSize, int difference, int miniRow, int miniCol, 
        List<IPossibilitySet> result, int start, BoxPositions current, ReadOnlyBitSet16 possibilities,
        bool excludeSameLine = false)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = solverData.MiniGridPositionsAt(miniRow, miniCol, i);
            if (pos.Count == 0) continue;

            var or = pos.Or(current);
            possibilities += i;

            if (or.Count == possibilities.Count + difference)
            {
                if(!excludeSameLine || !(or.AreAllInSameColumn() || or.AreAllInSameColumn())) 
                    result.Add(new SnapshotPossibilitySet(or.ToCellArray(),
                        possibilities, solverData.CurrentState));
            }

            if (possibilities.Count < maxSize)
                InMiniGrid(solverData, maxSize, difference, miniRow, miniCol, result, i + 1, or, possibilities);

            possibilities -= i;
        }
    }
}