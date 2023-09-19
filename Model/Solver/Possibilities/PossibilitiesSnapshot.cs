﻿using Model.Solver.Positions;

namespace Model.Solver.Possibilities;

public class PossibilitiesSnapshot : IPossibilitiesHolder //TODO add positions
{
    private readonly IPossibilities[,] _possibilities = new IPossibilities[9, 9];

    public static IPossibilitiesHolder TakeSnapshot(IPossibilitiesHolder holder)
    {
        PossibilitiesSnapshot snapshot = new PossibilitiesSnapshot();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                snapshot._possibilities[row, col] = holder.PossibilitiesAt(row, col).Copy();
            }
        }

        return snapshot;
    }

    public IReadOnlyPossibilities PossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col];
    }
    
    public IReadOnlyLinePositions RowPositionsAt(int row, int number)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (_possibilities[row, col].Peek(number)) result.Add(col);
        }
        return result;
    }

    public IReadOnlyLinePositions ColumnPositionsAt(int col, int number)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (_possibilities[row, col].Peek(number)) result.Add(row);
        }

        return result;
    }

    public IReadOnlyMiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number)
    {
        MiniGridPositions result = new(miniRow, miniCol);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var realRow = miniRow * 3 + i;
                var realCol = miniCol * 3 + j;
                
                if (_possibilities[realRow, realCol].Peek(number)) result.Add(i, j);
            }
        }

        return result;
    }
}