﻿using System;
using System.Collections;
using System.Collections.Generic;
using Model.Core;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.Position;

public class GridPositions : IReadOnlyGridPositions
{
    private const int FirstLimit = 53;
    private const ulong LongRowMask = 0x1FF;
    private const uint IntRowMask = 0x1FF;
    private const ulong LongColumnMask = 0x201008040201;
    private const uint IntColumnMask = 0x40201;
    private const ulong LongBoxMask = 0x1C0E07;
    private const uint IntBoxMask = 0x1C0E07;

    private ulong _first; //0 to 53 => First 6 boxes
    private uint _second; // 54 to 80 => Last 3 boxes

    public int Count =>
        System.Numerics.BitOperations.PopCount(_first) + System.Numerics.BitOperations.PopCount(_second);

    public GridPositions()
    {
        _first = 0;
        _second = 0;
    }

    public GridPositions(IEnumerable<Cell> cells)
    {
        _first = 0;
        _second = 0;

        foreach (var cell in cells)
        {
            Add(cell);
        }
    }

    private GridPositions(ulong first, uint second)
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
        if (n > FirstLimit) _second |= 1u << (n - FirstLimit - 1);
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
    
    public bool Contains(int row, int col)
    {
        int n = row * 9 + col;
        return n > FirstLimit ? ((_second >> (n - FirstLimit - 1)) & 1) > 0 : ((_first >> n) & 1) > 0;
    }

    public bool Contains(Cell coord)
    {
        return Contains(coord.Row, coord.Column);
    }

    public bool ContainsAny(GridPositions gp)
    {
        return (gp._first & _first) != 0ul || (gp._second & _second) != 0ul;
    }

    public bool ContainsEvery(GridPositions gp)
    {
        return (gp._first | _first) == gp._first && (gp._second | _second) == gp._second;
    }

    public void Remove(int row, int col)
    {
        int n = row * 9 + col;
        if (n > FirstLimit) _second &= ~(1u << (n - FirstLimit - 1));
        else _first &= ~(1ul << n);
    }

    public void Remove(Cell cell)
    {
        Remove(cell.Row, cell.Column);
    }
    
    public Cell First()
    {
        for (int row = _first > 0 ? 0 : 3; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (Contains(row, col)) return new Cell(row, col);
            }
        }

        return default;
    }

    public int RowCount(int row)
    {
        return row < 6
            ? System.Numerics.BitOperations.PopCount(_first & (LongRowMask << (row * 9)))
            : System.Numerics.BitOperations.PopCount(_second & (IntRowMask << ((row - 6) * 9)));
    }

    public bool IsRowNotEmpty(int row)
    {
        return row < 6
            ? (_first & (LongRowMask << (row * 9))) != 0
            : (_second & (IntRowMask << ((row - 6) * 9))) != 0;

    }

    public int ColumnCount(int column)
    {
        return System.Numerics.BitOperations.PopCount(_first & (LongColumnMask << column)) +
               System.Numerics.BitOperations.PopCount(_second & (IntColumnMask << column));
    }

    public bool IsColumnNotEmpty(int column)
    {
        return (_first & (LongColumnMask << column)) != 0 || (_second & (IntColumnMask << column)) != 0;
    }

    public int MiniGridCount(int miniRow, int miniCol)
    {
        return miniRow < 2
            ? System.Numerics.BitOperations.PopCount(_first & (LongBoxMask << (miniRow * 27 + miniCol * 3)))
            : System.Numerics.BitOperations.PopCount(_second & (IntBoxMask << (miniCol * 3)));
    }

    public bool IsMiniGridNotEmpty(int miniRow, int miniCol)
    {
        return miniRow < 2
            ? (_first & (LongBoxMask << (miniRow * 27 + miniCol * 3))) != 0
            : (_second & (IntBoxMask << (miniCol * 3))) != 0;
    }
    
    public LinePositions RowPositions(int row)
    {
        var i = row >= 6 ? (_second >> ((row - 6) * 9)) & IntRowMask : (_first >> (row * 9)) & LongRowMask;
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

    public BoxPositions MiniGridPositions(int miniRow, int miniCol)
    {
        var i = miniRow == 2 ? _second >> (miniCol * 3) : _first >> (miniRow * 27 + miniCol * 3);
        var j = (i & 0x7) | (i & 0xE00) >> 6 | (i & 0x1C0000) >> 12;
        return BoxPositions.FromBits(miniRow * 3, miniCol * 3, (int)j);
    }

    public void Fill()
    {
        _first = 0x3FFFFFFFFFFFFF;
        _second = 0x7FFFFFF;
    }

    public void Void()
    {
        _first = 0ul;
        _second = 0u;
    }

    public void FillRow(int row)
    {
        if (row < 6) _first |= LongRowMask << (row * 9);
        else _second |= IntRowMask << ((row - 6) * 9);
    }

    public void VoidRow(int row)
    {
        if (row < 6) _first &= ~(LongRowMask << (row * 9));
        else _second &= ~(IntRowMask << ((row - 6) * 9));
    }

    public void FillColumn(int column)
    {
        _first |= LongColumnMask << column;
        _second |= IntColumnMask << column;
    }

    public void VoidColumn(int column)
    {
        _first &= ~(LongColumnMask << column);
        _second &= ~(IntColumnMask << column);
    }

    public void FillMiniGrid(int miniRow, int miniCol)
    {
        if (miniRow < 2) _first |= LongBoxMask << (miniRow * 27 + miniCol * 3);
        else _second |= IntBoxMask << (miniCol * 3);
    }

    public void VoidMiniGrid(int miniRow, int miniCol)
    {
        if (miniRow < 2) _first &= ~(LongBoxMask << (miniRow * 27 + miniCol * 3));
        else _second &= ~(IntBoxMask << (miniCol * 3));
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
                if (Contains(row, col))
                {
                    result[cursor++] = new Cell(row, col);
                    if (cursor == result.Length) return result;
                }
            }
        }

        return result;
    }

    public bool CanBeCoveredByAUnit()
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

    public List<House> BestCoverHouses(params IUnitMethods[] methods)
    {
        List<House> result = new();
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

    public List<House[]> PossibleCoverHouses(int count, HashSet<House> forbidden, params IUnitMethods[] methods)
    {
        var result = new List<House[]>();

        PossibleCoverHouses(count, methods, this, result,
            new List<House>(), forbidden, new HashSet<int>());
        
        return result;
    }
    
    private void PossibleCoverHouses(int max, IUnitMethods[] methods, GridPositions gp,
        List<House[]> result, List<House> current, HashSet<House> forbidden,
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

    public List<CoveredGrid> PossibleCoveredGrids(int count, int maxRemaining, HashSet<House> forbidden,
        params IUnitMethods[] methods)
    {
        var result = new List<CoveredGrid>();

        PossibleCoveredGrids(count, maxRemaining, ToArray(), 0, methods, this, result,
            new List<House>(), forbidden, new HashSet<int>());

        return result;
    }

    private void PossibleCoveredGrids(int max, int maxRemaining, Cell[] cells, int start, IUnitMethods[] methods,
        GridPositions gp, List<CoveredGrid> result, List<House> current, HashSet<House> forbidden,
        HashSet<int> done)
    {
        for (int i = start; i < cells.Length; i++)
        {
            var c = cells[i];
            if (!gp.Contains(c)) continue;

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

    public IEnumerable<Cell> AllSeenCells()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (Contains(row, col)) continue;

                if (IsRowNotEmpty(row) || IsColumnNotEmpty(col) || IsMiniGridNotEmpty(row / 3, col / 3))
                    yield return new Cell(row, col);
            }
        }
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
                if (Contains(row, col)) result += $"r{row + 1}c{col + 1} ";
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
                if (Contains(row, col)) yield return new Cell(row, col);
            }
        }
    }
}

public record CoveredGrid(GridPositions Remaining, House[] CoverHouses);

public static class CoverHouseHelper
{
    public static int ToHash(IEnumerable<House> houses)
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