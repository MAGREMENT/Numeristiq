﻿using Model.Sudokus.Solver.Position;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility;

public class PositionsMap
{
    public LinePositions[] Rows { get; } = { new(), new(), new(), new(), new(), new(), new(), new(), new() };
    public LinePositions[] Columns { get; } = { new(), new(), new(), new(), new(), new(), new(), new(), new() };
    public BoxPositions[,] Minis { get; } =
    {
        { new(0, 0), new(0, 1), new(0, 2) },
        { new(1, 0), new(1, 1), new(1, 2) },
        { new(2, 0), new(2, 1), new(2, 2) }
    };
    
    public PositionsMap(ISudokuSolverData solverData, Filter filter)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (!filter(solverData.PossibilitiesAt(row, col))) continue;

                Rows[row].Add(col);
                Columns[col].Add(row);
                Minis[row / 3, col / 3].Add(row % 3, col % 3);
            }
        }
    }

    public delegate bool Filter(ReadOnlyBitSet16 possibilities);
}