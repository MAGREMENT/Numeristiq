using System.Collections.Generic;
using Global;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.PossibilityPosition;

public class CellsAndPossibilitiesPossibilitiesPositions : IPossibilitiesPositions
{
    private readonly Cell[] _cells;
    private readonly IPossibilitiesHolder _snapshot;
    private GridPositions? _gp;

    public CellsAndPossibilitiesPossibilitiesPositions(Cell[] cells, Possibilities possibilities, IPossibilitiesHolder snapshot)
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
        foreach (var cell in _cells)
        {
            if (_snapshot.PossibilitiesAt(cell.Row, cell.Col).Peek(possibility)) yield return cell;
        }
    }

    public IReadOnlyPossibilities PossibilitiesInCell(Cell cell)
    {
        return Possibilities.And(_snapshot.PossibilitiesAt(cell));
    }

    public Possibilities Possibilities { get; }

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
        throw new System.NotImplementedException();
    }

    public int PossibilityCount => Possibilities.Count;
    public int PositionsCount => _cells.Length;
}