using System.Collections;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Position;

public class BoxPositions : IReadOnlyBoxPositions
{
    private readonly int _startRow;
    private readonly int _startCol;

    private int _pos;
    
    public int Count { get; private set; }

    public BoxPositions(int miniRow, int miniCol)
    {
        _startRow = miniRow * 3;
        _startCol = miniCol * 3;
    }

    private BoxPositions(int pos, int count, int startRow, int startCol)
    {
        _pos = pos;
        Count = count;
        _startRow = startRow;
        _startCol = startCol;
    }
    
    public static BoxPositions Filled(int miniRow, int miniCol)
    {
        return new BoxPositions(0x1FF, 9, miniRow * 3, miniCol * 3);
    }

    public static BoxPositions FromBits(int miniRow, int miniCol, int bits)
    {
        return new BoxPositions(bits, System.Numerics.BitOperations.PopCount((uint)bits), miniRow, miniCol);
    }

    public void Add(int gridRow, int gridCol)
    {
        if (!Contains(gridRow, gridCol)) Count++;
        _pos |= 1 << (gridRow * 3 + gridCol);
    }
    
    public void Remove(int gridRow, int gridCol)
    {
        int delta = gridRow * 3 + gridCol;
        bool old = ((_pos >> delta) & 1) > 0;
        _pos &= ~(1 << delta);
        if (old) Count--;
    }
    
    public void Remove(int gridNumber)
    {
        bool old = ((_pos >> gridNumber) & 1) > 0;
        _pos &= ~(1 << gridNumber);
        if (old) Count--;
    }

    public void Void()
    {
        _pos = 0;
        Count = 0;
    }

    public void VoidGridRow(int gridRow)
    {
        _pos &= ~(0b111 << (gridRow * 3));
        Count = System.Numerics.BitOperations.PopCount((uint)_pos);
    }
    
    public void VoidGridColumn(int gridCol)
    {
        _pos &= ~(0b1001001 << gridCol);
        Count = System.Numerics.BitOperations.PopCount((uint)_pos);
    }

    public void Add(int gridNumber)
    {
        if (!Contains(gridNumber)) Count++;
        _pos |= 1 << gridNumber;
    }

    public bool Contains(int gridRow, int gridCol)
    {
        return ((_pos >> (gridRow * 3 + gridCol)) & 1) > 0;
    }

    public bool Contains(int gridNumber)
    {
        return ((_pos >> gridNumber) & 1) > 0;
    }

    public bool AreAllInSameRow()
    {
        //111 111 000
        //111 000 111
        //000 111 111
        return Count is < 4 and > 0 && ((_pos & 0x1F8) == 0 || (_pos & 0x1C7) == 0 || (_pos & 0x3F) == 0);
    }

    public bool AreAllInSameColumn()
    {
        //110 110 110
        //101 101 101
        //011 011 011
        return Count is < 4 and > 0 && ((_pos & 0x1B6) == 0 || (_pos & 0x16D) == 0 || (_pos & 0xDB) == 0);
    }

    public bool AtLeastOneInEachRows()
    {
        return System.Numerics.BitOperations.PopCount((uint)(_pos & 0b111)) > 0
                         && System.Numerics.BitOperations.PopCount((uint)(_pos & 0b111000)) > 0
                         && System.Numerics.BitOperations.PopCount((uint)(_pos & 0b111000000)) > 0;
    }
    
    public bool AtLeastOnInEachColumns()
    {
        return System.Numerics.BitOperations.PopCount((uint)(_pos & 0b1001001)) > 0
                         && System.Numerics.BitOperations.PopCount((uint)(_pos & 0b10010010)) > 0
                         && System.Numerics.BitOperations.PopCount((uint)(_pos & 0b100100100)) > 0;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not BoxPositions pos) return false;
        return _pos == pos._pos;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _pos;
    }

    public override string ToString()
    {
        return this.ToStringSequence(" ");
    }

    public (int, int) GetStarts() => (_startRow, _startCol);

    public LinePositions OnGridRow(int gridRow)
    {
        LinePositions pos = new();
        for (int i = 0; i < 3; i++)
        {
            if (((_pos >> (gridRow * 3 + i)) & 1) > 0) pos.Add(_startCol + i);
        }

        return pos;
    }

    public LinePositions OnGridColumn(int gridCol)
    {
        LinePositions pos = new();
        for (int i = 0; i < 3; i++)
        {
            if (((_pos >> (i * 3 + gridCol)) & 1) > 0) pos.Add(_startRow + i);
        }

        return pos;
    }

    public void Fill()
    {
        _pos = 0x1FF;
        Count = 9;
    }
    
    public Cell First()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Contains(i)) return new Cell(_startRow + i / 3, _startCol + i % 3);
        }

        return new Cell(-1, -1);
    }

    public Cell First(Cell except)
    {
        for (int i = 0; i < 9; i++)
        {
            if (!Contains(i)) continue;

            var current = new Cell(_startRow + i / 3, _startCol + i % 3);
            if (current != except) return current;
        }

        return new Cell(-1, -1);
    }

    public Cell Next(ref int cursor)
    {
        cursor++;
        for (; cursor < 9; cursor++)
        {
            if (Contains(cursor)) return new Cell(_startRow + cursor / 3, _startCol + cursor % 3);
        }

        return new Cell(-1, -1);
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Contains(i)) yield return new Cell(_startRow + i / 3, _startCol + i % 3);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public BoxPositions Difference(IReadOnlyBoxPositions pos)
    {
        if (pos is BoxPositions mgp)
        {
            var diff = _pos & ~mgp._pos;
            return new BoxPositions(diff, System.Numerics.BitOperations.PopCount((uint)diff), _startRow, _startCol);
        }

        return IReadOnlyBoxPositions.DefaultDifference(this, pos);
    }

    public BoxPositions Copy()
    {
        return new BoxPositions(_pos, Count, _startRow, _startCol);
    }

    public Cell[] ToCellArray()
    {
        var result = new Cell[Count];
        var cursor = 0;
        foreach (var cell in this)
        {
            result[cursor] = cell;
            cursor++;
        }

        return result;
    }

    public CellPossibility[] ToCellPossibilityArray(int digit)
    {
        var result = new CellPossibility[Count];
        var cursor = 0;
        foreach (var cell in this)
        {
            result[cursor] = new CellPossibility(cell, digit);
            cursor++;
        }

        return result;
    }

    public void ForEachCombination(IReadOnlyBoxPositions.HandleCombination handler)
    {
        for (int i = 0; i < 9; i++)
        {
            if (((_pos >> i) & 1) == 0) continue;
            for (int j = i + 1; j < 9; j++)
            {
                if (((_pos >> j) & 1) == 0) continue;
                handler(new Cell(
                    _startRow + i / 3, _startCol + i % 3),
                    new Cell(_startRow + j / 3, _startCol + j % 3));
            }
        }
    }

    public int GetNumber()
    {
        return _startRow + _startCol / 3;
    }
    
    public BoxPositions Or(IReadOnlyBoxPositions pos)
    {
        if (pos is not BoxPositions mini) return IReadOnlyBoxPositions.DefaultOr(pos, this);
        int newPos = _pos | mini._pos;
        return new BoxPositions(newPos, System.Numerics.BitOperations.PopCount((uint)newPos), _startRow,
            _startCol);
    }
    
}