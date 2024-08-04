using System;
using Model.Core.BackTracking;

namespace Model.Core;

public interface IBinarySolvingState
{
    int RowCount { get; }
    int ColumnCount { get; }
    int this[int row, int col] { get; }
}

public class DefaultBinarySolvingState : IBinarySolvingState, ICopyable<DefaultBinarySolvingState>
{
    private readonly ushort[,] _bits;

    public int RowCount => _bits.GetLength(0);
    public int ColumnCount => _bits.GetLength(1);

    public static DefaultBinarySolvingState Copy(IBinarySolvingState state)
    {
        return state is DefaultBinarySolvingState d ? d.Copy() : new DefaultBinarySolvingState(state);
    }

    public DefaultBinarySolvingState(int rowCount, int colCount)
    {
        _bits = new ushort[rowCount, colCount];
    }

    public DefaultBinarySolvingState(IBinarySolvingState state)
    {
        _bits = new ushort[state.RowCount, state.ColumnCount];
        for (int row = 0; row < state.RowCount; row++)
        {
            for (int col = 0; col < state.ColumnCount; col++)
            {
                _bits[row, col] = (ushort)state[row, col];
            }
        }
    }
    
    private DefaultBinarySolvingState(ushort[,] bits)
    {
        _bits = new ushort[bits.GetLength(0), bits.GetLength(1)];
        Array.Copy(bits, _bits, bits.Length);
    }

    public int this[int row, int col]
    {
        get => _bits[row, col];
        set => _bits[row, col] = (ushort)value;
    }

    public DefaultBinarySolvingState Copy() => new(_bits);
}