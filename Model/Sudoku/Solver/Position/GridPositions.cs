using System;
using System.Collections;
using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Utility;
using Model.Utility;

namespace Model.Sudoku.Solver.Position;

public class GridPositions : IReadOnlyGridPositions
{
    private const int FirstLimit = 53;
    private const ulong RowMask = 0x1FF;
    private const ulong FirstColumnMask = 0x201008040201;
    private const ulong SecondColumnMask = 0x40201;
    private const ulong MiniGridMask = 0x1C0E07;

    private ulong _first; //0 to 53 => First 6 boxes
    private ulong _second; // 54 to 80 => Last 3 boxes

    public int Count =>
        System.Numerics.BitOperations.PopCount(_first) + System.Numerics.BitOperations.PopCount(_second);

    public GridPositions()
    {
        _first = 0;
        _second = 0;
    }

    private GridPositions(ulong first, ulong second)
    {
        _first = first;
        _second = second;
    }

    public static GridPositions Filled()
    {
        return new GridPositions(0x3FFFFFFFFFFFFF, 0x7FFFFFF);
    }

    public void Add(int row, int col)
    {
        int n = row * 9 + col;
        if (n > FirstLimit) _second |= 1ul << (n - FirstLimit - 1);
        else _first |= 1ul << n;
    }

    public void Add(Cell coord)
    {
        Add(coord.Row, coord.Column);
    }

    public void Add(IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            Add(cell);
        }
    }
    
    public bool Peek(int row, int col)
    {
        int n = row * 9 + col;
        return n > FirstLimit ? ((_second >> (n - FirstLimit - 1)) & 1) > 0 : ((_first >> n) & 1) > 0;
    }

    public bool Peek(Cell coord)
    {
        return Peek(coord.Row, coord.Column);
    }

    public bool PeakAny(GridPositions gp)
    {
        return (gp._first & _first) != 0ul || (gp._second & _second) != 0ul;
    }

    public void Remove(int row, int col)
    {
        int n = row * 9 + col;
        if (n > FirstLimit) _second &= ~(1ul << (n - FirstLimit - 1));
        else _first &= ~(1ul << n);
    }

    public void Remove(Cell cell)
    {
        Remove(cell.Row, cell.Column);
    }
    
    public Cell First()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (Peek(row, col)) return new Cell(row, col);
            }
        }

        return default;
    }

    public int RowCount(int row)
    {
        return row < 6
            ? System.Numerics.BitOperations.PopCount(_first & (RowMask << (row * 9)))
            : System.Numerics.BitOperations.PopCount(_second & (RowMask << ((row - 6) * 9)));
    }

    public int ColumnCount(int column)
    {
        return System.Numerics.BitOperations.PopCount(_first & (FirstColumnMask << column)) +
               System.Numerics.BitOperations.PopCount(_second & (SecondColumnMask << column));
    }

    public int MiniGridCount(int miniRow, int miniCol)
    {
        return miniRow < 2
            ? System.Numerics.BitOperations.PopCount(_first & (MiniGridMask << (miniRow * 27 + miniCol * 3)))
            : System.Numerics.BitOperations.PopCount(_second & (MiniGridMask << (miniCol * 3)));
    }
    
    public LinePositions RowPositions(int row)
    {
        var i = row >= 6 ? (_second >> ((row - 6) * 9)) & RowMask : (_first >> (row * 9)) & RowMask;
        return LinePositions.FromBits((int)i);
    }

    public LinePositions ColumnPositions(int col)
    {
        var i = _first >> col;
        var j = _second >> col;
        var k = i & 1 | (i & 0x200) >> 8 | (i & 0x40000) >> 16 |
                (i & 0x8000000) >> 24 | (i & 0x1000000000) >> 32 | (i & 0x200000000000) >> 40 |
                (j & 1) << 6 | (j & 0x200) >> 2 | (j & 0x40000) >> 10;
        return LinePositions.FromBits((int)k);
    }

    public MiniGridPositions MiniGridPositions(int miniRow, int miniCol)
    {
        var i = miniRow == 2 ? _second >> (miniCol * 3) : _first >> (miniRow * 27 + miniCol * 3);
        var j = (i & 0x7) | (i & 0xE00) >> 6 | (i & 0x1C0000) >> 12;
        return Position.MiniGridPositions.FromBits(miniRow * 3, miniCol * 3, (int)j);
    }

    public void Fill()
    {
        _first = 0x3FFFFFFFFFFFFF;
        _second = 0x7FFFFFF;
    }

    public void Void()
    {
        _first = 0ul;
        _second = 0ul;
    }

    public void FillRow(int row)
    {
        if (row < 6) _first |= RowMask << (row * 9);
        else _second |= RowMask << ((row - 6) * 9);
    }

    public void VoidRow(int row)
    {
        if (row < 6) _first &= ~(RowMask << (row * 9));
        else _second &= ~(RowMask << ((row - 6) * 9));
    }

    public void FillColumn(int column)
    {
        _first |= FirstColumnMask << column;
        _second |= SecondColumnMask << column;
    }

    public void VoidColumn(int column)
    {
        _first &= ~(FirstColumnMask << column);
        _second &= ~(SecondColumnMask << column);
    }

    public void FillMiniGrid(int miniRow, int miniCol)
    {
        if (miniRow < 2) _first |= MiniGridMask << (miniRow * 27 + miniCol * 3);
        else _second |= MiniGridMask << (miniCol * 3);
    }

    public void VoidMiniGrid(int miniRow, int miniCol)
    {
        if (miniRow < 2) _first &= ~(MiniGridMask << (miniRow * 27 + miniCol * 3));
        else _second &= ~(MiniGridMask << (miniCol * 3));
    }

    public GridPositions Copy()
    {
        return new GridPositions(_first, _second);
    }

    public GridPositions And(IReadOnlyGridPositions p)
    {
        if (p is GridPositions other)
        {
            return new GridPositions(_first & other._first, _second & other._second);
        }

        return IReadOnlyGridPositions.DefaultAnd(this, p);
    }

    public void ApplyAnd(GridPositions gp)
    {
        _first &= gp._first;
        _second &= gp._second;
    }

    public void ApplyAnd(IReadOnlyGridPositions gp)
    {
        if (gp is GridPositions o) ApplyAnd(o);
    }

    public GridPositions Or(IReadOnlyGridPositions pos)
    {
        if (pos is GridPositions other)
        {
            return new GridPositions(_first | other._first, _second | other._second);
        }

        return IReadOnlyGridPositions.DefaultOr(this, pos);
    }

    public void ApplyOr(GridPositions gp)
    {
        _first |= gp._first;
        _second |= gp._second;
    }

    public GridPositions And(List<GridPositions> gps)
    {
        var first = _first;
        var second = _second;

        foreach (var gp in gps)
        {
            first &= gp._first;
            second &= gp._second;
        }

        return new GridPositions(first, second);
    }
    
    public GridPositions And(params GridPositions[] gps)
    {
        var first = _first;
        var second = _second;

        foreach (var gp in gps)
        {
            first &= gp._first;
            second &= gp._second;
        }

        return new GridPositions(first, second);
    }

    public GridPositions Or(GridPositions positions)
    {
        return new GridPositions(_first | positions._first, _second | positions._second);
    }

    public GridPositions Or(List<GridPositions> gps)
    {
        var first = _first;
        var second = _second;

        foreach (var gp in gps)
        {
            first |= gp._first;
            second |= gp._second;
        }

        return new GridPositions(first, second);
    }

    public GridPositions Difference(IReadOnlyGridPositions with)
    {
        if (with is not GridPositions gp) return IReadOnlyGridPositions.DefaultDifference(this, with);
        return new GridPositions(_first & ~gp._first, _second & ~gp._second);
    }

    public Cell[] ToArray()
    {
        var result = new Cell[Count];
        var cursor = 0;

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (Peek(row, col))
                {
                    result[cursor++] = new Cell(row, col);
                    if (cursor == result.Length) return result;
                }
            }
        }

        return result;
    }

    public bool CanBeCoverByAUnit()
    {
        if (Count == 0) return true;

        var first = First();
        return RowCount(first.Row) == Count || ColumnCount(first.Column) == Count ||
               MiniGridCount(first.Row / 3, first.Column / 3) == Count;
    }

    public bool CanBeCoveredByUnits(int n, params Unit[] units)
    {
        if (Count == 0) return true;

        var copy = Copy();
        int[] counts = new int[units.Length];
        for (int i = 0; i < n; i++)
        {
            var first = copy.First();

            for (int j = 0; j < units.Length; j++)
            {
                counts[j] = UnitMethods.Get(units[j]).Count(copy, first);
            }

            UnitMethods.Get(units[MathUtility.MaxIndex(counts)]).Void(copy, first);

            if (copy.Count == 0) return true;
        }

        return false;
    }

    public List<CoverHouse> BestCoverHouses(params IUnitMethods[] methods)
    {
        List<CoverHouse> result = new();
        var copy = Copy();

        while (copy.Count > 0)
        {
            Cell bCell = default;
            var bCount = 0;
            var bMethod = methods.Length + 1;

            foreach (var cell in copy)
            {
                var bestCount = 0;
                var bestMethod = methods.Length + 1;

                for (int i = 0; i < methods.Length; i++)
                {
                    var count = methods[i].Count(copy, cell);

                    if (count > bestCount || (count == bestCount && i < bestMethod))
                    {
                        bestCount = count;
                        bestMethod = i;
                    }
                }

                if (bestCount > bCount || (bestCount == bCount && bestMethod < bMethod))
                {
                    bCount = bestCount;
                    bMethod = bestMethod;
                    bCell = cell;
                }
            }

            var m = methods[bMethod];
            m.Void(copy, bCell);

            result.Add(m.ToCoverHouse(bCell));
        }

        return result;
    }

    public List<CoverHouse[]> PossibleCoverHouses(int count, HashSet<CoverHouse> forbidden, params IUnitMethods[] methods)
    {
        var result = new List<CoverHouse[]>();

        PossibleCoverHouses(count, methods, this, result,
            new List<CoverHouse>(), forbidden, new HashSet<int>());
        
        return result;
    }
    
    private void PossibleCoverHouses(int max, IUnitMethods[] methods, GridPositions gp,
        List<CoverHouse[]> result, List<CoverHouse> current, HashSet<CoverHouse> forbidden,
        HashSet<int> done)
    {
        var first = gp.First();
        foreach (var method in methods)
        {
            var ch = method.ToCoverHouse(first);
            if (forbidden.Contains(ch)) continue;
            
            current.Add(ch);
            var hash = CoverHouseHelper.ToHash(current);
            if (done.Contains(hash))
            {
                current.RemoveAt(current.Count - 1);
                continue;
            }
            done.Add(hash);
            
            var copy = gp.Copy();
            method.Void(copy, first);

            if (copy.Count == 0) result.Add(current.ToArray());
            else if (current.Count < max) PossibleCoverHouses(max, methods, copy, result, current, forbidden, done);
            
            current.RemoveAt(current.Count - 1);
        }
    }

    public List<CoveredGrid> PossibleCoveredGrids(int count, int maxRemaining, HashSet<CoverHouse> forbidden,
        params IUnitMethods[] methods)
    {
        var result = new List<CoveredGrid>();

        PossibleCoveredGrids(count, maxRemaining, ToArray(), 0, methods, this, result,
            new List<CoverHouse>(), forbidden, new HashSet<int>());

        return result;
    }

    private void PossibleCoveredGrids(int max, int maxRemaining, Cell[] cells, int start, IUnitMethods[] methods,
        GridPositions gp, List<CoveredGrid> result, List<CoverHouse> current, HashSet<CoverHouse> forbidden,
        HashSet<int> done)
    {
        for (int i = start; i < cells.Length; i++)
        {
            var c = cells[i];
            if (!gp.Peek(c)) continue;

            foreach (var method in methods)
            {
                var ch = method.ToCoverHouse(c);
                if (forbidden.Contains(ch)) continue;
                
                current.Add(ch);
                var hash = CoverHouseHelper.ToHash(current);
                if (done.Contains(hash))
                {
                    current.RemoveAt(current.Count - 1);
                    continue;
                }
                done.Add(hash);

                var copy = gp.Copy();
                method.Void(copy, c);
                
                if(copy.Count == 0) result.Add(new CoveredGrid(copy, current.ToArray()));
                else if (current.Count == max)
                {
                    if(copy.Count <= maxRemaining) result.Add(new CoveredGrid(copy, current.ToArray()));
                }
                else if (current.Count < max) PossibleCoveredGrids(max, maxRemaining, cells, i + 1,
                        methods, copy, result, current, forbidden, done);

                current.RemoveAt(current.Count - 1);
            }
        }
    }

    public Cell[] AllSeenCells()
    {
        var result = new GridPositions();

        foreach (var cell in this)
        {
            result.FillRow(cell.Row);
            result.FillColumn(cell.Column);
            result.FillMiniGrid(cell.Row / 3, cell.Column / 3);
        }

        foreach (var cell in this)
        {
            result.Remove(cell);
        }

        return result.ToArray();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_first, _second);
    }

    public override bool Equals(object? obj)
    {
        return obj is GridPositions gp && gp._second == _second && gp._first == _first;
    }

    public override string ToString()
    {
        var result = "";
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (Peek(row, col)) result += $"r{row + 1}c{col + 1} ";
            }
        }

        return result;
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (Peek(row, col)) yield return new Cell(row, col);
            }
        }
    }
}

public record CoveredGrid(GridPositions Remaining, CoverHouse[] CoverHouses);

public static class CoverHouseHelper
{
    public static int ToHash(IReadOnlyList<CoverHouse> houses)
    {
        int hash = 0;
        foreach (var house in houses)
        {
            var delta = house.Number + 9 * (int)house.Unit;
            hash |= 1 << delta;
        }

        return hash;
    }
}