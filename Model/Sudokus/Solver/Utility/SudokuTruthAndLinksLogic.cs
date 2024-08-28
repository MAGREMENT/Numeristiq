using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility;

public static class SudokuTruthAndLinksLogic
{
    public static void AddTruthAndLinks(ISudokuSolverData data, ITruthAndLinkBank<Cell,
        ITruthOrLink<Cell>> bank, int digit, bool isTruth)
    {
        for (int u = 0; u < 9; u++)
        {
            var lp = data.RowPositionsAt(u, digit);
            if(lp.Count > 0) bank.Add(new HouseCellsSudokuTruthOrLink(new House(Unit.Row, u),
                lp.ToCellArray(Unit.Row, u)), isTruth);
            
            lp = data.ColumnPositionsAt(u, digit);
            if(lp.Count > 0) bank.Add(new HouseCellsSudokuTruthOrLink(new House(Unit.Column, u),
                lp.ToCellArray(Unit.Column, u)), isTruth);

            var bp = data.MiniGridPositionsAt(u / 3, u % 3, digit);
            if(bp.Count > 0) bank.Add(new HouseCellsSudokuTruthOrLink(new House(Unit.Box, u),
                bp.ToCellArray()), isTruth);
        }
    }
}

public class HouseCellsSudokuTruthOrLink : ITruthOrLink<Cell>
{
    public House House { get; }
    private readonly Cell[] _cells;

    public HouseCellsSudokuTruthOrLink(House house, Cell[] cells)
    {
        House = house;
        _cells = cells;
    }

    public bool DoesOverlap(ITruthOrLink<Cell> link)
    {
        if (link is HouseCellsSudokuTruthOrLink h && !h.House.Overlaps(House)) return false;
        return TruthAndLinksLogic.DefaultDoesOverlap(this, link);
    }

    public bool Contains(Cell element)
    {
        return _cells.Contains(element);
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        foreach (var cell in _cells) yield return cell;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override int GetHashCode()
    {
        return House.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is HouseCellsSudokuTruthOrLink h && h.House == House;
    }

    public override string ToString()
    {
        return House.ToString();
    }
}

public class HouseCellPossibilitiesSudokuTruthOrLink : ITruthOrLink<CellPossibility>
{
    public House House { get; }
    private readonly CellPossibility[] _cells;

    public HouseCellPossibilitiesSudokuTruthOrLink(House house, CellPossibility[] cells)
    {
        House = house;
        _cells = cells;
    }

    public bool DoesOverlap(ITruthOrLink<CellPossibility> link)
    {
        if (link is HouseCellPossibilitiesSudokuTruthOrLink h && !h.House.Overlaps(House)) return false;
        return TruthAndLinksLogic.DefaultDoesOverlap(this, link);
    }

    public bool Contains(CellPossibility element)
    {
        return _cells.Contains(element);
    }

    public IEnumerator<CellPossibility> GetEnumerator()
    {
        foreach (var cell in _cells) yield return cell;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override int GetHashCode()
    {
        return House.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is HouseCellPossibilitiesSudokuTruthOrLink h && h.House == House;
    }
    
    public override string ToString()
    {
        return House.ToString();
    }
}