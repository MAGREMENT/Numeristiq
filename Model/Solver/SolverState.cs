using System;
using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibility;

namespace Model.Solver;

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
                else _cellStates[row, col] = solver.PossibilitiesAt(row, col).ToCellState();
            }
        }
    }

    public SolverState()
    {
        _cellStates = new CellState[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for(int col = 0; col < 9; col++)
            {
                _cellStates[row, col] = CellState.FromBits(0x1FF);
            }
        }
    }

    public SolverState(CellState[,] cellStates)
    {
        if (cellStates.Length != 81) throw new ArgumentException("Not enough cells");
        _cellStates = cellStates;
    }

    public CellState At(int row, int col)
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

    public Possibilities PossibilitiesAt(int row, int col)
    {
        return At(row, col).AsPossibilities;
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
                poss.Remove(change.Number);
                buffer[change.Row, change.Column] = poss.ToCellState();
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
                        poss.Remove(change.Number);
                        buffer[unit, change.Column] = poss.ToCellState();
                    }

                    current = buffer[change.Row, unit];
                    if (current.IsPossibilities)
                    {
                        var poss = current.AsPossibilities;
                        poss.Remove(change.Number);
                        buffer[change.Row, unit] = poss.ToCellState();
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
                            poss.Remove(change.Number);
                            buffer[row, col] = poss.ToCellState();
                        }
                    }
                }
            }
        }

        return new SolverState(buffer);
    }
}

public readonly struct CellState
{
    private readonly short _bits;

    public CellState(int solved)
    {
        _bits = (short) (solved << 10);
    }

    private CellState(short bits)
    {
        _bits = bits;
    }

    public static CellState FromBits(short bits)
    {
        return new CellState(bits);
    }

    public bool IsPossibilities => _bits <= 0x3FE;
    
    public Possibilities AsPossibilities => Possibilities.FromBits(_bits & 0x3FE);

    public int AsNumber => _bits >> 10;
}