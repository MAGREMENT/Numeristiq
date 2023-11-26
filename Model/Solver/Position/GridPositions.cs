using System;
using System.Collections;
using System.Collections.Generic;
using Global;
using Model.Utility;

namespace Model.Solver.Position;

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

    public void Add(int row, int col)
    {
        int n = row * 9 + col;
        if (n > FirstLimit) _second |= 1ul << (n - FirstLimit - 1);
        else _first |= 1ul << n;
    }

    public void Add(Cell coord)
    {
        Add(coord.Row, coord.Col);
    }

    public bool Peek(int row, int col)
    {
        int n = row * 9 + col;
        return n > FirstLimit ? ((_second >> (n - FirstLimit - 1)) & 1) > 0 : ((_first >> n) & 1) > 0;
    }

    public bool Peek(Cell coord)
    {
        return Peek(coord.Row, coord.Col);
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
        Remove(cell.Row, cell.Col);
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

    public void Fill()
    {
        _first = 0x3FFFFFFFFFFFFF;
        _second = 0x7FFFFFF;
    }

    public void Reset()
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

    public bool CanBeCoveredByLines(int n, params Unit[] units)
    {
        if (Count == 0) return true;

        var copy = Copy();
        int[] counts = new int[units.Length];
        for (int i = 0; i < n; i++)
        {
            var first = copy.First();

            for (int j = 0; j < units.Length; j++)
            {
                counts[j] = UnitMethods.GetMethods(units[j]).Count(copy, first);
            }

            UnitMethods.GetMethods(units[MathUtility.MaxIndex(counts)]).Void(copy, first);

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

    public List<CoveredGrid> PossibleCoverHouses(int count, HashSet<CoverHouse> forbidden, params IUnitMethods[] methods)
    {
        var result = new List<CoveredGrid>();

        PossibleCoverHouses(count, methods, 1, this, result,
            new List<CoverHouse>(), forbidden);
        
        return result;
    }

    private const int FinnedLimit = 3;
    
    private void PossibleCoverHouses(int max, IUnitMethods[] methods, int count, GridPositions gp,
        List<CoveredGrid> result, List<CoverHouse> current, HashSet<CoverHouse> forbidden)
    {
        var first = gp.First();
        foreach (var method in methods)
        {
            var ch = method.ToCoverHouse(first);
            if (forbidden.Contains(ch)) continue;
            
            var copy = gp.Copy();
            method.Void(copy, first);
            current.Add(ch);

            if (copy.Count == 0 || (count == max && copy.Count <= FinnedLimit))
                result.Add(new CoveredGrid(current.ToArray(), copy));
            if (count < max) PossibleCoverHouses(max, methods, count + 1, copy, result, current, forbidden);
            
            current.RemoveAt(current.Count - 1);
        }
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

    public override int GetHashCode()
    {
        return HashCode.Combine(_first, _second);
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
}

public static class UnitMethods
{
    public static readonly IUnitMethods[] AllUnitMethods = { new RowMethods(), new ColumnMethods(), new MiniGridMethods() };

    public static IUnitMethods GetMethods(Unit u)
    {
        return AllUnitMethods[(int)u];
    }
}

public interface IUnitMethods
{
    int Count(IReadOnlyGridPositions gp, Cell c);

    int Count(IReadOnlyGridPositions gp, int unit);

    void Fill(GridPositions gp, Cell c);
    
    void Fill(GridPositions gp, int unit);

    void Void(GridPositions gp, Cell c);
    
    void Void(GridPositions gp, int unit);

    CoverHouse ToCoverHouse(Cell cell);
}

public class RowMethods : IUnitMethods
{
    public int Count(IReadOnlyGridPositions gp, Cell c)
    {
        return gp.RowCount(c.Row);
    }

    public int Count(IReadOnlyGridPositions gp, int unit)
    {
        return gp.RowCount(unit);
    }

    public void Fill(GridPositions gp, Cell c)
    {
        gp.FillRow(c.Row);
    }

    public void Fill(GridPositions gp, int unit)
    {
        gp.FillRow(unit);
    }

    public void Void(GridPositions gp, Cell c)
    { 
        gp.VoidRow(c.Row);
    }

    public void Void(GridPositions gp, int unit)
    {
        gp.VoidRow(unit);
    }

    public CoverHouse ToCoverHouse(Cell cell)
    {
        return new CoverHouse(Unit.Row, cell.Row);
    }
}

public class ColumnMethods : IUnitMethods
{
    public int Count(IReadOnlyGridPositions gp, Cell c)
    {
        return gp.ColumnCount(c.Col);
    }

    public int Count(IReadOnlyGridPositions gp, int unit)
    {
        return gp.ColumnCount(unit);
    }

    public void Fill(GridPositions gp, Cell c)
    {
        gp.FillColumn(c.Col);
    }

    public void Fill(GridPositions gp, int unit)
    {
        gp.FillColumn(unit);
    }

    public void Void(GridPositions gp, Cell c)
    { 
        gp.VoidColumn(c.Col);
    }

    public void Void(GridPositions gp, int unit)
    {
        gp.VoidColumn(unit);
    }
    
    public CoverHouse ToCoverHouse(Cell cell)
    {
        return new CoverHouse(Unit.Column, cell.Col);
    }
}

public class MiniGridMethods : IUnitMethods
{
    public int Count(IReadOnlyGridPositions gp, Cell c)
    {
        return gp.MiniGridCount(c.Row / 3, c.Col / 3);
    }

    public int Count(IReadOnlyGridPositions gp, int unit)
    {
        return gp.MiniGridCount(unit / 3, unit % 3);
    }

    public void Fill(GridPositions gp, Cell c)
    {
        gp.FillMiniGrid(c.Row / 3, c.Col / 3);
    }

    public void Fill(GridPositions gp, int unit)
    {
        gp.FillMiniGrid(unit / 3, unit % 3);
    }

    public void Void(GridPositions gp, Cell c)
    { 
        gp.VoidMiniGrid(c.Row / 3, c.Col / 3);
    }

    public void Void(GridPositions gp, int unit)
    {
        gp.VoidMiniGrid(unit / 3, unit % 3);
    }

    public CoverHouse ToCoverHouse(Cell cell)
    {
        return new CoverHouse(Unit.MiniGrid, cell.Row / 3 * 3 + cell.Col / 3);
    }
}

public readonly struct CoverHouse
{
    public CoverHouse(Unit unit, int number)
    {
        Unit = unit;
        Number = number;
    }

    public Unit Unit { get; }
    public int Number { get; }

    public override bool Equals(object? obj)
    {
        return obj is CoverHouse ch && ch == this;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Unit, Number);
    }

    public override string ToString()
    {
        var s = Unit switch
        {
            Unit.Row => "r",
            Unit.Column => "c",
            Unit.MiniGrid => "b"
        };

        return $"{s}{Number + 1}";
    }

    public static bool operator ==(CoverHouse left, CoverHouse right)
    {
        return left.Unit == right.Unit && left.Number == right.Number;
    }

    public static bool operator !=(CoverHouse left, CoverHouse right)
    {
        return !(left == right);
    }

    public bool Overlaps(CoverHouse house)
    {
        switch (Unit, house.Unit)
        {
            case (Unit.Row, Unit.Column) :
            case (Unit.Column, Unit.Row) :
                return true;
            case (Unit.Row, Unit.MiniGrid) :
            case (Unit.MiniGrid, Unit.Row) :
                return Number / 3 == house.Number / 3;
            case (Unit.Column, Unit.MiniGrid) :
                return Number / 3 == house.Number % 3;
            case (Unit.MiniGrid, Unit.Column) :
                return Number % 3 == Number / 3;
            case (Unit.Row, Unit.Row) :
            case (Unit.Column, Unit.Column) :
            case (Unit.MiniGrid, Unit.MiniGrid) :
                return Number == house.Number;
        }

        return false;
    }
}

public record CoveredGrid(CoverHouse[] CoverHouses, GridPositions Remaining);