using System;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.Symmetry;

public abstract class SudokuSymmetry
{
    public void ApplyOnceFullSymmetry(ISudokuSolverData solverData, int[] mapping)
    {
        var selfMap = SelfMapped(mapping);

        foreach (var cell in CenterCells())
        {
            foreach (var possibility in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (!selfMap.Contains(possibility))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                }
            }
        }
    }
    
    public void ApplyEveryTimeFullSymmetry(ISudokuSolverData solverData, int[] mapping)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var symmetry = GetSymmetricalCell(row, col);
                var solved = solverData.Sudoku[row, col];

                if (solved != 0) solverData.ChangeBuffer.ProposeSolutionAddition(mapping[solved - 1],
                    symmetry.Row, symmetry.Column);
                else
                {
                    var symmetricPossibilities = solverData.PossibilitiesAt(symmetry);
                    var mappedPossibilities = Paired(solverData.PossibilitiesAt(row, col), mapping);

                    foreach (var possibility in symmetricPossibilities.EnumeratePossibilities())
                    {
                        if(!mappedPossibilities.Contains(possibility)) solverData.ChangeBuffer
                            .ProposePossibilityRemoval(possibility, symmetry.Row, symmetry.Column);
                    }
                }
            }
        }
    }

    public int[]? CheckFullSymmetry(ISudokuSolverData solverData)
    {
        var mapping = new int[9];

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = solverData.Sudoku[row, col];
                if (solved == 0) continue;

                var symmetricCell = GetSymmetricalCell(row, col);

                var symmetry = solverData.Sudoku[symmetricCell.Row, symmetricCell.Column];
                if (symmetry == 0) return null;

                var definedSymmetry = mapping[solved - 1];
                if (definedSymmetry == 0) mapping[solved - 1] = symmetry;
                else if (definedSymmetry != symmetry) return null;
            }
        }

        int count = 0;
        for (int i = 0; i < mapping.Length; i++)
        {
            var symmetry = mapping[i];
            if (symmetry == 0) return null;
            if (symmetry == i + 1) count++;
        }

        return count >= MinimumSelfMapCount ? mapping : null;
    }

    public AlmostSymmetryResult? CheckAlmostSymmetry(ISudokuSolverData solverData)
    {
        var mapping = new int[9];
        Cell? exception = null;

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = solverData.Sudoku[row, col];
                if (solved == 0) continue;

                var symmetricCell = GetSymmetricalCell(row, col);

                var symmetry = solverData.Sudoku[symmetricCell.Row, symmetricCell.Column];
                if (symmetry == 0)
                {
                    if (exception is not null) return null;

                    exception = new Cell(row, col);
                }

                var definedSymmetry = mapping[solved - 1];
                if (definedSymmetry == 0) mapping[solved - 1] = symmetry;
                else if (definedSymmetry != symmetry) return null;
            }
        }

        int count = 0;
        for (int i = 0; i < mapping.Length; i++)
        {
            var symmetry = mapping[i];
            if (symmetry == 0) return null;
            if (symmetry == i + 1) count++;
        }

        return exception is not null 
            ? new AlmostSymmetryResult(mapping, count, exception.Value)
            : null;
    }
    
    public Cell GetSymmetricalCell(Cell c) => GetSymmetricalCell(c.Row, c.Column);
    
    public static SudokuSymmetry[] All() => new SudokuSymmetry[]
    {
        new NegativeDiagonalSudokuSymmetry(), new PositiveDiagonalSudokuSymmetry(),
        new Rotational90SudokuSymmetry(), new Rotational180SudokuSymmetry(), new Rotational270SudokuSymmetry(),
        new ColumnSticksSudokuSymmetry(), new RowSticksSudokuSymmetry()
    };

    public abstract int MinimumSelfMapCount { get; }
    public abstract int MaximumSelfMapCount { get; }
    public abstract IEnumerable<Cell> CenterCells();
    protected abstract Cell GetSymmetricalCell(int row, int col);

    public static ReadOnlyBitSet16 SelfMapped(IReadOnlyList<int> mapping)
    {
        var selfMap = new ReadOnlyBitSet16();
        for (int i = 0; i < mapping.Count; i++)
        {
            if (mapping[i] == i + 1) selfMap += i + 1;
        }

        return selfMap;
    }

    private static ReadOnlyBitSet16 Paired(ReadOnlyBitSet16 possibilities, IReadOnlyList<int> mapping)
    {
        var result = new ReadOnlyBitSet16();
        foreach (var possibility in possibilities.EnumeratePossibilities())
        {
            result += mapping[possibility - 1];
        }

        return result;
    }

}

public record AlmostSymmetryResult(int[] Mapping, int SelfMapCount, Cell Exception);

public class NegativeDiagonalSudokuSymmetry : SudokuSymmetry
{
    public override int MinimumSelfMapCount => 3;
    public override int MaximumSelfMapCount => 9;

    public override IEnumerable<Cell> CenterCells()
    {
        for (int unit = 0; unit < 9; unit++)
        {
            yield return new Cell(unit, unit);
        }
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(col, row);
    }
}

public class PositiveDiagonalSudokuSymmetry : SudokuSymmetry
{
    public override int MinimumSelfMapCount => 3;
    public override int MaximumSelfMapCount => 9;

    public override IEnumerable<Cell> CenterCells()
    {
        for (int unit = 0; unit < 9; unit++)
        {
            yield return new Cell(unit, 8 - unit);
        }
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(8 - col, 8 - row);
    }
}

public class Rotational90SudokuSymmetry : SudokuSymmetry
{ 
    public override int MinimumSelfMapCount => 1;
    public override int MaximumSelfMapCount => 1;
    
    public override IEnumerable<Cell> CenterCells()
    {
        yield return new Cell(4, 4);
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(col, 8 - row);
    }
}

public class Rotational180SudokuSymmetry : SudokuSymmetry
{
    public override int MinimumSelfMapCount => 1;
    public override int MaximumSelfMapCount => 1;
    
    public override IEnumerable<Cell> CenterCells()
    {
        yield return new Cell(4, 4);
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(8 - row, 8 - col);
    }
}

public class Rotational270SudokuSymmetry : SudokuSymmetry
{
    public override int MinimumSelfMapCount => 1;
    public override int MaximumSelfMapCount => 1;
    
    public override IEnumerable<Cell> CenterCells()
    {
        yield return new Cell(4, 4);
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        return new Cell(8 - col, row);
    }
}

public class ColumnSticksSudokuSymmetry : SudokuSymmetry
{
    public override int MinimumSelfMapCount => 3;
    public override int MaximumSelfMapCount => 9;
    public override IEnumerable<Cell> CenterCells()
    {
        yield return new Cell(3, 1);
        yield return new Cell(4, 1);
        yield return new Cell(5, 1);
        yield return new Cell(3, 4);
        yield return new Cell(4, 4);
        yield return new Cell(5, 4);
        yield return new Cell(3, 7);
        yield return new Cell(4, 7);
        yield return new Cell(5, 7);
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        var r = (row / 3) switch
        {
            0 => row + 6,
            1 => row,
            2 => row - 6,
            _ => throw new ArgumentOutOfRangeException()
        };
        var c = (col % 3) switch
        {
            0 => col + 2,
            1 => col,
            2 => col - 2,
            _ => throw new ArgumentOutOfRangeException()
        };

        return new Cell(r, c);
    }
}

public class RowSticksSudokuSymmetry : SudokuSymmetry
{
    public override int MinimumSelfMapCount => 3;
    public override int MaximumSelfMapCount => 9;
    public override IEnumerable<Cell> CenterCells()
    {
        yield return new Cell(1, 3);
        yield return new Cell(1, 4);
        yield return new Cell(1, 5);
        yield return new Cell(4, 3);
        yield return new Cell(4, 4);
        yield return new Cell(4, 5);
        yield return new Cell(7, 3);
        yield return new Cell(7, 4);
        yield return new Cell(7, 5);
    }

    protected override Cell GetSymmetricalCell(int row, int col)
    {
        var r = (row % 3) switch
        {
            0 => row + 2,
            1 => row,
            2 => row - 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        var c = (col / 3) switch
        {
            0 => col + 6,
            1 => col,
            2 => col - 6,
            _ => throw new ArgumentOutOfRangeException()
        };

        return new Cell(r, c);
    }
}