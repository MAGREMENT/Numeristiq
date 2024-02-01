using System.Collections.Generic;
using Model.Utility;

namespace Model.Tectonic;

public class TectonicSolver : ISolvable
{
    private ITectonic _tectonic;
    private readonly Dictionary<Cell, Candidates> _candidates = new();

    public TectonicSolver()
    {
        _tectonic = new BlankTectonic();
    }

    public void SetTectonic(ITectonic tectonic)
    {
        _tectonic = tectonic;
        InitCandidates();
    }

    private void InitCandidates()
    {
        _candidates.Clear();

        foreach (var cell in _tectonic.EachCell())
        {
            _candidates[cell] = new Candidates(_tectonic.GetZone(cell).Count);
        }

        foreach (var cellNumber in _tectonic.EachCellNumber())
        {
            if (!cellNumber.IsSet()) continue;

            foreach (var neighbor in _tectonic.GetNeighbors(cellNumber.Cell))
            {
                _candidates[neighbor].Remove(cellNumber.Number);
            }

            foreach (var cell in _tectonic.GetZone(cellNumber.Cell))
            {
                _candidates[cell].Remove(cellNumber.Number);
            }
        }
    }

    public IReadOnlyTectonic Tectonic => _tectonic;

    public Candidates GetCandidates(Cell cell)
    {
        return _candidates[cell];
    }
}