using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public class KakuroNumericSolvingState : IUpdatableNumericSolvingState
{
    private readonly ushort[,] _bits;
    private readonly IEnumerable<IKakuroSum> _sums;
    private readonly IKakuroCombinationCalculator _calculator;

    public KakuroNumericSolvingState(KakuroSolver solver)
    {
        _bits = new ushort[solver.Kakuro.RowCount, solver.Kakuro.ColumnCount];
        foreach (var cell in solver.Kakuro.EnumerateCells())
        {
            var n = solver.Kakuro[cell.Row, cell.Column];
            if (n == 0) _bits[cell.Row, cell.Column] = solver.PossibilitiesAt(cell.Row, cell.Column).Bits;
            else _bits[cell.Row, cell.Column] = (ushort)((n << 1) | 1);
        }
        
        _sums = solver.Kakuro.Sums;
        _calculator = solver.CombinationCalculator;
    }

    private KakuroNumericSolvingState(ushort[,] bits, IEnumerable<IKakuroSum> sums, IKakuroCombinationCalculator calculator)
    {
        _bits = bits;
        _sums = sums;
        _calculator = calculator;
    }
    
    public int this[int row, int col]
    {
        get
        {
            var b = _bits[row, col];
            return (b & 1) > 0 ? b >> 1 : 0;
        }
    }

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        var b = _bits[row, col];
        return (b & 1) > 0 ? new ReadOnlyBitSet16() : ReadOnlyBitSet16.FromBits(b);
    }

    public IUpdatableNumericSolvingState Apply(IEnumerable<NumericChange> progresses)
    {
        var buffer = new ushort[_bits.GetLength(0), _bits.GetLength(1)];
        Array.Copy(_bits, 0, buffer, 0, _bits.Length);

        foreach (var progress in progresses)
        {
            ApplyToBuffer(buffer, progress);
        }

        return new KakuroNumericSolvingState(buffer, _sums, _calculator);
    }

    public IUpdatableNumericSolvingState Apply(NumericChange progress)
    {
        var buffer = new ushort[_bits.GetLength(0), _bits.GetLength(1)];
        Array.Copy(_bits, 0, buffer, 0, _bits.Length);

        ApplyToBuffer(buffer, progress);

        return new KakuroNumericSolvingState(buffer, _sums, _calculator);
    }

    private void ApplyToBuffer(ushort[,] buffer, NumericChange progress)
    {
        if (progress.Type == ChangeType.PossibilityRemoval)
        {
            buffer[progress.Row, progress.Column] &= (ushort)~(1 << progress.Number);
        }
        else
        {
            buffer[progress.Row, progress.Column] = (ushort)((progress.Number << 1) | 1);
            var current = new Cell(progress.Row, progress.Column);
            
            foreach (var sum in _sums)
            {
                if (!sum.Contains(current)) continue;

                List<int> solutions = new();
                foreach (var c in sum)
                {
                    var n = _bits[c.Row, c.Column];
                    if ((n & 1) > 0) solutions.Add(n >> 1);
                }
                
                var pos = _calculator.CalculatePossibilities(sum.Amount,
                    sum.Length, solutions);

                foreach (var cell in sum)
                {
                    if (cell.Row == current.Row && cell.Column == current.Column) continue;

                    _bits[cell.Row, cell.Column] &= pos.Bits;
                }
            }
        }
    }
}