using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Sudokus.Solver.Position;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.States;

public class StateArraySolvingState : IUpdatableSudokuSolvingState
{
    private readonly CellState[,] _cellStates;

    public StateArraySolvingState(INumericSolvingState solver)
    {
        _cellStates = new CellState[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for(int col = 0; col < 9; col++)
            {
                if (solver[row, col] != 0) _cellStates[row, col] = new CellState(solver[row, col]);
                else _cellStates[row, col] = CellState.FromBits(solver.PossibilitiesAt(row, col).Bits);
            }
        }
    }

    public StateArraySolvingState()
    {
        _cellStates = new CellState[9, 9];
    }

    private StateArraySolvingState(CellState[,] cellStates)
    {
        _cellStates = cellStates;
    }

    public void Set(int row, int col, CellState state)
    {
        _cellStates[row, col] = state;
    }

    public int this[int row, int col]
    {
        get
        {
            var current = _cellStates[row, col];
            return current.IsPossibilities ? 0 : current.AsNumber;
        } 
    }

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        return _cellStates[row, col].AsPossibilities;
    }

    public IUpdatableNumericSolvingState Apply(IEnumerable<NumericChange> changes)
    {
        var buffer = new CellState[9, 9];
        Array.Copy(_cellStates, 0, buffer, 0, _cellStates.Length);

        foreach (var change in changes)
        {
            ApplyProgressToBuffer(buffer, change);
        }

        return new StateArraySolvingState(buffer);
    }

    public IUpdatableNumericSolvingState Apply(NumericChange progress)
    {
        var buffer = new CellState[9, 9];
        Array.Copy(_cellStates, 0, buffer, 0, _cellStates.Length);
        
        ApplyProgressToBuffer(buffer, progress);
        
        return new StateArraySolvingState(buffer);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not StateArraySolvingState state) return false;

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (_cellStates[row, col] != state._cellStates[row, col]) return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        return _cellStates.GetHashCode();
    }

    private void ApplyProgressToBuffer(CellState[,] buffer, NumericChange numericNumericChange)
    {
        if (numericNumericChange.Type == ChangeType.PossibilityRemoval)
        {
            var poss = buffer[numericNumericChange.Row, numericNumericChange.Column].AsPossibilities;
            poss -= numericNumericChange.Number;
            buffer[numericNumericChange.Row, numericNumericChange.Column] = CellState.FromBits(poss.Bits);
        }
        else
        {
            buffer[numericNumericChange.Row, numericNumericChange.Column] = new CellState(numericNumericChange.Number);
                        
            for (int unit = 0; unit < 9; unit++)
            {
                var current = buffer[unit, numericNumericChange.Column];
                if (current.IsPossibilities)
                {
                    var poss = current.AsPossibilities;
                    poss -= numericNumericChange.Number;
                    buffer[unit, numericNumericChange.Column] = CellState.FromBits(poss.Bits);
                }
        
                current = buffer[numericNumericChange.Row, unit];
                if (current.IsPossibilities)
                {
                    var poss = current.AsPossibilities;
                    poss -= numericNumericChange.Number;
                    buffer[numericNumericChange.Row, unit] = CellState.FromBits(poss.Bits);
                }
            }
        
            for (int mr = 0; mr < 3; mr++)
            {
                for (int mc = 0; mc < 3; mc++)
                {
                    var row = numericNumericChange.Row / 3 * 3 + mr;
                    var col = numericNumericChange.Column / 3 * 3 + mc;
                                
                    var current = buffer[row, col];
                    if (current.IsPossibilities)
                    {
                        var poss = current.AsPossibilities;
                        poss -= numericNumericChange.Number;
                        buffer[row, col] = CellState.FromBits(poss.Bits);
                    }
                }
            }
        }
    }

    public IReadOnlyLinePositions ColumnPositionsAt(int col, int number)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (_cellStates[row, col].AsPossibilities.Contains(number)) result.Add(row);
        }

        return result;
    }

    public IReadOnlyLinePositions RowPositionsAt(int row, int number)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (_cellStates[row, col].AsPossibilities.Contains(number)) result.Add(col);
        }

        return result;
    }

    public IReadOnlyMiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number)
    {
        MiniGridPositions result = new(miniRow, miniCol);

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (_cellStates[miniRow * 3 + r, miniCol * 3 + c].AsPossibilities.Contains(number)) result.Add(r, c);
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
                if (_cellStates[row, col].AsPossibilities.Contains(number)) result.Add(row, col);
            }
        }

        return result;
    }
}

public readonly struct CellState
{
    private readonly ushort _bits;

    public CellState(int solved)
    {
        _bits = (ushort) (solved << 10);
    }

    private CellState(ushort bits)
    {
        _bits = bits;
    }

    public static CellState FromBits(ushort bits)
    {
        return new CellState(bits);
    }

    public bool IsPossibilities => _bits <= 0x3FE;
    
    public ReadOnlyBitSet16 AsPossibilities => ReadOnlyBitSet16.FromBits((ushort) (_bits & 0x3FE));

    public int AsNumber => _bits >> 10;

    public override bool Equals(object? obj)
    {
        return obj is CellState cs && cs._bits == _bits;
    }

    public override int GetHashCode()
    {
        return _bits;
    }

    public static bool operator ==(CellState left, CellState right)
    {
        return left._bits == right._bits;
    }
    
    public static bool operator !=(CellState left, CellState right)
    {
        return left._bits != right._bits;
    }
}