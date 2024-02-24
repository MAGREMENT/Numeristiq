using System;
using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.Possibility;

namespace Model.Sudoku.Solver;

public class SolverState : ITranslatable
{
    private readonly CellState[,] _cellStates;

    public SolverState(IPossibilitiesHolder solver)
    {
        _cellStates = new CellState[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for(int col = 0; col < 9; col++)
            {
                if (solver.Sudoku[row, col] != 0) _cellStates[row, col] = new CellState(solver.Sudoku[row, col]);
                else _cellStates[row, col] = CellState.FromBits(solver.PossibilitiesAt(row, col).Bits);
            }
        }
    }

    public SolverState()
    {
        _cellStates = new CellState[9, 9];
    }

    private SolverState(CellState[,] cellStates)
    {
        _cellStates = cellStates;
    }

    public void Set(int row, int col, CellState state)
    {
        _cellStates[row, col] = state;
    }

    public CellState Get(int row, int col)
    {
        return _cellStates[row, col];
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
        return Get(row, col).AsPossibilities;
    }

    public SolverState Apply(IReadOnlyList<SolverChange> changes)
    {
        var buffer = new CellState[9, 9];
        Array.Copy(_cellStates, 0, buffer, 0, _cellStates.Length);

        foreach (var change in changes)
        {
            if (change.ChangeType == ChangeType.Possibility)
            {
                var poss = buffer[change.Row, change.Column].AsPossibilities;
                poss -= change.Number;
                buffer[change.Row, change.Column] = CellState.FromBits(poss.Bits);
            }
            else
            {
                buffer[change.Row, change.Column] = new CellState(change.Number);
                
                for (int unit = 0; unit < 9; unit++)
                {
                    var current = buffer[unit, change.Column];
                    if (current.IsPossibilities)
                    {
                        var poss = current.AsPossibilities;
                        poss -= change.Number;
                        buffer[unit, change.Column] = CellState.FromBits(poss.Bits);
                    }

                    current = buffer[change.Row, unit];
                    if (current.IsPossibilities)
                    {
                        var poss = current.AsPossibilities;
                        poss -= change.Number;
                        buffer[change.Row, unit] = CellState.FromBits(poss.Bits);
                    }
                }

                for (int mr = 0; mr < 3; mr++)
                {
                    for (int mc = 0; mc < 3; mc++)
                    {
                        var row = change.Row / 3 * 3 + mr;
                        var col = change.Column / 3 * 3 + mc;
                        
                        var current = buffer[row, col];
                        if (current.IsPossibilities)
                        {
                            var poss = current.AsPossibilities;
                            poss -= change.Number;
                            buffer[row, col] = CellState.FromBits(poss.Bits);
                        }
                    }
                }
            }
        }

        return new SolverState(buffer);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not SolverState state) return false;

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