using System;

namespace Model.Core;

public interface IDichotomousSolvingState
{
    int RowCount { get; }
    int ColumnCount { get; }
    bool this[int row, int col] { get; }
    bool IsAvailable(int row, int col);
}

public interface INonogramSolvingState : IDichotomousSolvingState
{
    public int GetRowSolutionCount(int row);
    public int GetColumnSolutionCount(int column);
}

public class DefaultDichotomousSolvingState : INonogramSolvingState
{
    private readonly ushort[,] _bits;

    public static DefaultDichotomousSolvingState Copy(IDichotomousSolvingState state)
    {
        return state is DefaultDichotomousSolvingState s
            ? s.Copy()
            : new DefaultDichotomousSolvingState(state);
    }
    
    public DefaultDichotomousSolvingState(int rowCount, int colCount)
    {
        _bits = new ushort[rowCount, colCount];
    }
    
    public DefaultDichotomousSolvingState(IDichotomousSolvingState state)
    {
        _bits = new ushort[state.RowCount, state.ColumnCount];
        for (int row = 0; row < state.RowCount; row++)
        {
            for (int col = 0; col < state.ColumnCount; col++)
            {
                this[row, col] = state[row, col];
                SetAvailability(row, col, state.IsAvailable(row, col));
            }
        }
    }

    private DefaultDichotomousSolvingState(ushort[,] bits)
    {
        _bits = new ushort[bits.GetLength(0), bits.GetLength(1)];
        Array.Copy(bits, _bits, bits.Length);
    }
    
    public int RowCount => _bits.GetLength(0);
    public int ColumnCount => _bits.GetLength(1);

    public bool this[int row, int col]
    {
        get => (_bits[row, col] & 1) > 0;
        set
        {
            if (value) _bits[row, col] |= 1;
            else _bits[row, col] = (ushort)(_bits[row, col] & ~1);
        }
    }

    public bool IsAvailable(int row, int col)
    {
        return _bits[row, col] >= 2;
    }

    public void SetAvailability(int row, int col, bool a)
    {
        if (a) _bits[row, col] |= 2;
        else _bits[row, col] = (ushort)(_bits[row, col] & ~2);
    }
    
    public int GetRowSolutionCount(int row)
    {
        var current = 0;
        for (int c = 0; c < RowCount; c++)
        {
            if (this[row, c]) current++;
        }

        return current;
    }

    public int GetColumnSolutionCount(int column)
    {
        var current = 0;
        for (int r = 0; r < RowCount; r++)
        {
            if (this[r, column]) current++;
        }

        return current;
    }
    
    public DefaultDichotomousSolvingState Copy() => new(_bits);
}