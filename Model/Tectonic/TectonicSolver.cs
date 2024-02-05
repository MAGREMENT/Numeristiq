using System;
using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Tectonic;

public class TectonicSolver : IStrategyUser, IChangeProducer
{
    private ITectonic _tectonic;
    private ReadOnlyBitSet16[,] _possibilities;

    public IChangeBuffer ChangeBuffer { get; }

    public TectonicSolver()
    {
        _tectonic = new BlankTectonic();
        _possibilities = new ReadOnlyBitSet16[0, 0];

        ChangeBuffer = new FastChangeBuffer(this);
    }

    public void SetTectonic(ITectonic tectonic)
    {
        _tectonic = tectonic;
        _possibilities = new ReadOnlyBitSet16[_tectonic.RowCount, _tectonic.ColumnCount];
        InitCandidates();
    }

    public IReadOnlyTectonic Tectonic => _tectonic;

    public ReadOnlyBitSet16 PossibilitiesAt(Cell cell)
    {
        return _possibilities[cell.Row, cell.Column];
    }
    
    public ReadOnlyBitSet16 ZonePositionsFor(int zone, int n)
    {
        var result = new ReadOnlyBitSet16();
        var z = _tectonic.Zones[zone];

        for (int i = 0; i < z.Count; i++)
        {
            var cell = z[i];
            if (_possibilities[cell.Row, cell.Column].Contains(n)) result += n;
        }

        return result;
    }

    public bool CanRemovePossibility(CellPossibility cp)
    {
        return _possibilities[cp.Row, cp.Column].Contains(cp.Possibility);
    }

    public bool CanAddSolution(CellPossibility cp)
    {
        return _tectonic[cp.Row, cp.Column] == 0;
    }

    public bool ExecuteChange(SolverChange change)
    {
        if (change.ChangeType == ChangeType.Possibility)
        {
            var yes = _possibilities[change.Row, change.Column].Contains(change.Number);
            if(yes) _possibilities[change.Row, change.Column] -= change.Number;
            return yes;
        }

        if (_tectonic[change.Row, change.Column] != 0) return false;
        _tectonic[change.Row, change.Column] = change.Number;
        UpdatePossibilitiesAfterSolutionAdded(change.Row, change.Column, change.Number);
        return true;
    }
    
    //Private-----------------------------------------------------------------------------------------------------------
    
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

            UpdatePossibilitiesAfterSolutionAdded(cellNumber.Cell.Row, cellNumber.Cell.Column, cellNumber.Number);
        }
    }

    private void UpdatePossibilitiesAfterSolutionAdded(int row, int col, int number)
    {
        _possibilities[row, col] = new ReadOnlyBitSet16();

        foreach (var neighbor in _tectonic.GetNeighbors(row, col))
        {
            _possibilities[neighbor.Row, neighbor.Column] -= number;
        }

        foreach (var cell in _tectonic.GetZone(row, col))
        {
            _possibilities[cell.Row, cell.Column] -= number;
        }
    }
}