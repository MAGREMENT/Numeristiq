using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.BitSets;
using Model.Utility;

namespace Model.Tectonic;

public class TectonicSolver : IStrategyUser
{
    private ITectonic _tectonic;
    private ReadOnlyBitSet16[,] _possibilities;

    public TectonicSolver()
    {
        _tectonic = new BlankTectonic();
        _possibilities = new ReadOnlyBitSet16[0, 0];
    }

    public void SetTectonic(ITectonic tectonic)
    {
        _tectonic = tectonic;
        _possibilities = new ReadOnlyBitSet16[_tectonic.RowCount, _tectonic.ColumnCount];
        InitCandidates();
    }

    private void InitCandidates()
    {
        foreach (var zone in _tectonic.Zones)
        {
            foreach (var cell in zone)
            {
                _possibilities[cell.Row, cell.Column] = ReadOnlyBitSet16.Filled(1, zone.Count);
            }
        }

        foreach (var cellNumber in _tectonic.EachCellNumber())
        {
            if (!cellNumber.IsSet()) continue;

            foreach (var neighbor in _tectonic.GetNeighbors(cellNumber.Cell))
            {
                _possibilities[neighbor.Row, neighbor.Column] -= cellNumber.Number;
            }

            foreach (var cell in _tectonic.GetZone(cellNumber.Cell))
            {
                _possibilities[cell.Row, cell.Column] -= cellNumber.Number;
            }
        }
    }

    public IReadOnlyTectonic Tectonic => _tectonic;

    public ReadOnlyBitSet16 PossibilitiesAt(Cell cell)
    {
        return _possibilities[cell.Row, cell.Column];
    }
}