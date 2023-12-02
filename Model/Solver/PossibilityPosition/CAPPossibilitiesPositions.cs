using System;
using System.Collections.Generic;
using System.Text;
using Global;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.PossibilityPosition;

public class CAPPossibilitiesPositions : IPossibilitiesPositions
{
    private readonly Cell[] _cells;
    private readonly IPossibilitiesHolder _snapshot;
    private GridPositions? _gp;
    
    public Possibilities Possibilities { get; }
    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cells.Length;

    public CAPPossibilitiesPositions(Cell[] cells, Possibilities possibilities, IPossibilitiesHolder snapshot)
    {
        _cells = cells;
        Possibilities = possibilities;
        _snapshot = snapshot;
    }
    
    
    public IEnumerable<int> EachPossibility()
    {
        return Possibilities;
    }

    public IEnumerable<Cell> EachCell()
    {
        return _cells;
    }

    public IEnumerable<Cell> EachCell(int possibility)
    {
        foreach(var cell in _cells)
        {
            if (_snapshot.PossibilitiesAt(cell.Row, cell.Column).Peek(possibility)) yield return cell;
        }
    }

    public IReadOnlyPossibilities PossibilitiesInCell(Cell cell)
    {
        return Possibilities.And(_snapshot.PossibilitiesAt(cell));
    }

    public GridPositions Positions
    {
        get
        {
            if (_gp is null)
            {
                _gp = new GridPositions();
                foreach (var cell in _cells)
                {
                    _gp.Add(cell);
                }
            }

            return _gp;
        }
    }

    public CellPossibilities[] ToCellPossibilitiesArray()
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        return obj is CAPPossibilitiesPositions pp && Possibilities.Equals(pp.Possibilities)
                                                                     && Positions.Equals(pp.Positions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibilities.GetHashCode(), Positions.GetHashCode());
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var pos in Possibilities)
        {
            builder.Append(pos);
        }

        builder.Append("{ ");
        foreach(var cell in EachCell())
        {
            builder.Append(cell + " ");
        }

        builder.Append('}');

        return builder.ToString();
    }
}