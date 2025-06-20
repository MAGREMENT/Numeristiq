using System;
using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Core;

public interface INumericSolvingState
{
    int RowCount { get; }
    int ColumnCount { get; }
    int this[int row, int col] { get; }
    ReadOnlyBitSet16 PossibilitiesAt(int row, int col);
    ReadOnlyBitSet16 PossibilitiesAt(Cell cell) => PossibilitiesAt(cell.Row, cell.Column);

    List<int> GetSolutions(IEnumerable<Cell> cells)
    {
        List<int> result = new();
        foreach (var cell in cells)
        {
            var n = this[cell.Row, cell.Column];
            if (n != 0) result.Add(n);
        }

        return result;
    }
}

public interface ISudokuSolvingState : INumericSolvingState
{
    public bool ContainsAny(int row, int col, ReadOnlyBitSet16 possibilities)
    {
        var solved = this[row, col];
        return solved == 0 ? PossibilitiesAt(row, col).ContainsAny(possibilities) : possibilities.Contains(solved);
    }

    public bool ContainsAny(Cell cell, ReadOnlyBitSet16 possibilities)
    {
        return ContainsAny(cell.Row, cell.Column, possibilities);
    }

    public bool Contains(int row, int col, int possibility)
    {
        var solved = this[row, col];
        return solved == 0 ? PossibilitiesAt(row, col).Contains(possibility) : solved == possibility;
    }
    
    IReadOnlyLinePositions ColumnPositionsAt(int col, int number);

    IReadOnlyLinePositions RowPositionsAt(int row, int number);

    IReadOnlyBoxPositions MiniGridPositionsAt(int miniRow, int miniCol, int number);

    IReadOnlyGridPositions PositionsFor(int number);
}

public class DefaultNumericSolvingState : ISudokuSolvingState
{
    private readonly ushort[,] _bits;

    public static DefaultNumericSolvingState Copy(INumericSolvingState state)
    {
        return state is DefaultNumericSolvingState s
            ? s.Copy()
            : new DefaultNumericSolvingState(state.RowCount, state.ColumnCount, state);
    }
    
    public DefaultNumericSolvingState(int rowCount, int colCount)
    {
        _bits = new ushort[rowCount, colCount];
    }
    
    public DefaultNumericSolvingState(int rowCount, int colCount, INumericSolvingState state)
    {
        _bits = new ushort[rowCount, colCount];
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                var number = state[row, col];
                if (number == 0) SetPossibilitiesAt(row, col, state.PossibilitiesAt(row, col));
                else this[row, col] = number;
            }
        }
    }

    private DefaultNumericSolvingState(ushort[,] bits)
    {
        _bits = new ushort[bits.GetLength(0), bits.GetLength(1)];
        Array.Copy(bits, _bits, bits.Length);
    }

    public int RowCount => _bits.GetLength(0);
    public int ColumnCount => _bits.GetLength(1);

    public int this[int row, int col]
    {
        get
        {
            var b = _bits[row, col];
            return (b & 1) > 0 ? b >> 1 : 0;
        }

        set => _bits[row, col] = (ushort)(value << 1 | 1);
    }

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        var b = _bits[row, col];
        return (b & 1) > 0 ? new ReadOnlyBitSet16() : ReadOnlyBitSet16.FromBits(b);
    }

    public void SetPossibilitiesAt(int row, int col, ReadOnlyBitSet16 bitSet)
    {
        _bits[row, col] = bitSet.Bits;
    }
    
    public void SetPossibilitiesAt(int row, int col, ReadOnlyBitSet8 bitSet)
    {
        _bits[row, col] = bitSet.Bits;
    }

    public void RemovePossibility(int p, int row, int col)
    {
        if((_bits[row, col] & 1) == 0) _bits[row, col] &= (ushort)~(1 << p);
    }

    public void AndPossibilities(ReadOnlyBitSet16 set, int row, int col)
    {
        if ((_bits[row, col] & 1) == 0) _bits[row, col] &= set.Bits;
    }

    public DefaultNumericSolvingState Copy() => new(_bits);
    
    public IReadOnlyLinePositions ColumnPositionsAt(int col, int number)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (PossibilitiesAt(row, col).Contains(number)) result.Add(row);
        }

        return result;
    }

    public IReadOnlyLinePositions RowPositionsAt(int row, int number)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (PossibilitiesAt(row, col).Contains(number)) result.Add(col);
        }

        return result;
    }

    public IReadOnlyBoxPositions MiniGridPositionsAt(int miniRow, int miniCol, int number)
    {
        BoxPositions result = new(miniRow, miniCol);

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (PossibilitiesAt(miniRow * 3 + r, miniCol * 3 + c).Contains(number)) result.Add(r, c);
            }
        }

        return result;
    }

    public IReadOnlyGridPositions PositionsFor(int number)
    {
        GridPositions result = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (PossibilitiesAt(row, col).Contains(number)) result.Add(row, col);
            }
        }

        return result;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DefaultNumericSolvingState s || s._bits.GetLength(0) != _bits.GetLength(0) 
                                                    || s._bits.GetLength(1) != _bits.GetLength(1)) return false;
        for(int i = 0; i < _bits.GetLength(0); i++)
        {
            for(int j = 0; j < _bits.GetLength(1); j++)
            {
                if (s._bits[i, j] != _bits[i, j]) return false;
            }
        }

        return true;
    }
    
    public override int GetHashCode()
    {
        return _bits.GetHashCode();
    }
}