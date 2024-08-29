using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility;

public static class SudokuTruthAndLinksLogic
{
    public static void SetUpForMSLS(ISudokuSolverData data, ITruthAndLinkBank<CellPossibility,
        ITruthOrLink<CellPossibility>> bank)
    {
        AddCellTruthAndLinks(data, bank, true);
        for (int n = 1; n <= 9; n++)
        {
            AddHouseTruthAndLinks(data, bank, n, false);
        }
    }
    
    public static void AddCellTruthAndLinks(ISudokuSolverData data, ITruthAndLinkBank<CellPossibility,
        ITruthOrLink<CellPossibility>> bank, bool isTruth)
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                var poss = data.PossibilitiesAt(r, c);
                if(poss.Count > 0) bank.Add(new MultiDigitCellTruthOrLink(new Cell(r, c), poss.ToArray()),
                    isTruth);
            }
        }
    }
    
    public static void AddHouseTruthAndLinks(ISudokuSolverData data, ITruthAndLinkBank<CellPossibility,
        ITruthOrLink<CellPossibility>> bank, int digit, bool isTruth)
    {
        for (int u = 0; u < 9; u++)
        {
            var lp = data.RowPositionsAt(u, digit);
            if(lp.Count > 0) bank.Add(new HouseCellPossibilitySudokuTruthOrLink(new House(Unit.Row, u),
                lp.ToCellPossibilityArray(Unit.Row, u, digit)), isTruth);
            
            lp = data.ColumnPositionsAt(u, digit);
            if(lp.Count > 0) bank.Add(new HouseCellPossibilitySudokuTruthOrLink(new House(Unit.Column, u),
                lp.ToCellPossibilityArray(Unit.Column, u, digit)), isTruth);

            var bp = data.MiniGridPositionsAt(u / 3, u % 3, digit);
            if(bp.Count > 0) bank.Add(new HouseCellPossibilitySudokuTruthOrLink(new House(Unit.Box, u),
                bp.ToCellPossibilityArray(digit)), isTruth);
        }
    }
    
    public static void AddHouseTruthAndLinks(ISudokuSolverData data, ITruthAndLinkBank<Cell,
        HouseCellsSudokuTruthOrLink> bank, int digit, bool isTruth)
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
    
    public static void AddHouseTruthAndLinks(ISudokuSolverData data, ITruthAndLinkBank<Cell,
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

public class HouseCellPossibilitySudokuTruthOrLink : ITruthOrLink<CellPossibility>
{
    public House House { get; }
    private readonly CellPossibility[] _cells;

    public HouseCellPossibilitySudokuTruthOrLink(House house, CellPossibility[] cells)
    {
        House = house;
        _cells = cells;
    }

    public bool DoesOverlap(ITruthOrLink<CellPossibility> link)
    {
        if (link is HouseCellPossibilitySudokuTruthOrLink h && !h.House.Overlaps(House)) return false;
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
        return obj is HouseCellPossibilitySudokuTruthOrLink h && h.House == House;
    }
    
    public override string ToString()
    {
        return House.ToString();
    }
}

public class MultiDigitCellTruthOrLink : ITruthOrLink<CellPossibility>
{
    private readonly Cell _cell;
    private readonly int[] _possibilities;

    public MultiDigitCellTruthOrLink(Cell cell, int[] possibilities)
    {
        _cell = cell;
        _possibilities = possibilities;
    }

    public IEnumerator<CellPossibility> GetEnumerator()
    {
        foreach (var p in _possibilities)
        {
            yield return new CellPossibility(_cell, p);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool DoesOverlap(ITruthOrLink<CellPossibility> link)
    {
        return TruthAndLinksLogic.DefaultDoesOverlap(this, link);
    }

    public bool Contains(CellPossibility element)
    {
        return _cell == element.ToCell() && _possibilities.Contains(element.Possibility);
    }

    public override bool Equals(object? obj)
    {
        return obj is MultiDigitCellTruthOrLink m && m._cell == _cell && m._possibilities.SequenceEqual(_possibilities);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_cell.GetHashCode(), _possibilities.GetHashCode());
    }

    public override string ToString()
    {
        return _cell.ToString();
    }
}