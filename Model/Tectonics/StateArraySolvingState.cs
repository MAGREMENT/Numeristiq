using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Tectonics.Solver;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics;

public class StateArraySolvingState : IUpdatableTectonicSolvingState
{
    private readonly ushort[,] _bits;
    private readonly IReadOnlyList<IZone> _zones;

    public StateArraySolvingState(ITectonicSolverData solver)
    {
        _bits = new ushort[solver.Tectonic.RowCount, solver.Tectonic.ColumnCount];

        for (int row = 0; row < solver.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < solver.Tectonic.ColumnCount; col++)
            {
                var poss = solver.PossibilitiesAt(row, col);
                _bits[row, col] = poss.Count == 0 ? (ushort)((solver.Tectonic[row, col] << 1) | 1) : poss.Bits;
            }
        }

        _zones = solver.Tectonic.Zones;
    }

    private StateArraySolvingState(ushort[,] buffer, IReadOnlyList<IZone> zones)
    {
        _bits = buffer;
        _zones = zones;
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

    public IUpdatableSolvingState Apply(IReadOnlyList<NumericChange> progresses)
    {
        var buffer = new ushort[_bits.GetLength(0), _bits.GetLength(1)];
        Array.Copy(_bits, 0, buffer, 0, _bits.Length);

        foreach (var progress in progresses)
        {
            ApplyToBuffer(buffer, progress);
        }

        return new StateArraySolvingState(buffer, _zones);
    }

    public IUpdatableSolvingState Apply(NumericChange progress)
    {
        var buffer = new ushort[_bits.GetLength(0), _bits.GetLength(1)];
        Array.Copy(_bits, 0, buffer, 0, _bits.Length);

        ApplyToBuffer(buffer, progress);

        return new StateArraySolvingState(buffer, _zones);
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

            foreach (var neighbor in TectonicCellUtility.GetNeighbors(progress.Row, progress.Column,
                         buffer.GetLength(0), buffer.GetLength(1)))
            {
                if((buffer[neighbor.Row, neighbor.Column] & 1) == 0) 
                    buffer[neighbor.Row, neighbor.Column] &= (ushort)~(1 << progress.Number);
            }

            IZone? zone = null;
            foreach (var z in _zones)
            {
                if (z.Contains(new Cell(progress.Row, progress.Column)))
                {
                    zone = z;
                    break;
                }
            }

            if (zone is null) return;
            
            foreach (var cell in zone)
            {
                if((buffer[cell.Row, cell.Column] & 1) == 0) 
                    buffer[cell.Row, cell.Column] &= (ushort)~(1 << progress.Number);
            }
        }
    }
}