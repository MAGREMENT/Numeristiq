using System.Collections;
using System.Collections.Generic;
using System.Text;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Positions;

public class MiniGridPositions : IReadOnlyMiniGridPositions
{
    private readonly int _startRow;
    private readonly int _startCol;

    private int _pos;
    
    public int Count { get; private set; }

    public MiniGridPositions(int miniRow, int miniCol)
    {
        _startRow = miniRow * 3;
        _startCol = miniCol * 3;
    }

    private MiniGridPositions(int pos, int count, int startRow, int startCol)
    {
        _pos = pos;
        Count = count;
        _startRow = startRow;
        _startCol = startCol;
    }

    public static MiniGridPositions FromBits(int miniRow, int miniCol, int bits)
    {
        return new MiniGridPositions(bits, System.Numerics.BitOperations.PopCount((uint)bits), miniRow, miniCol);
    }

    public void Add(int gridRow, int gridCol)
    {
        if (!Peek(gridRow, gridCol)) Count++;
        _pos |= 1 << (gridRow * 3 + gridCol);
    }
    
    public void Remove(int gridRow, int gridCol)
    {
        int delta = gridRow * 3 + gridCol;
        bool old = ((_pos >> delta) & 1) > 0;
        _pos &= ~(1 << delta);
        if (old) Count--;
    }

    public void Void()
    {
        _pos = 0;
        Count = 0;
    }

    public void Add(int gridNumber)
    {
        _pos |= 1 << gridNumber;
    }

    public bool Peek(int gridRow, int gridCol)
    {
        return ((_pos >> (gridRow * 3 + gridCol)) & 1) > 0;
    }

    public bool Peek(int gridNumber)
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
    
    public override bool Equals(object? obj)
    {
        if (obj is not MiniGridPositions pos) return false;
        return _pos == pos._pos;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _pos;
    }

    public override string ToString()
    {
        var builder = new StringBuilder("(");
        foreach (var coord in this)
        {
            builder.Append(coord);
        }

        return builder.ToString()[..^1] + ")";
    }

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
    
    public Cell GetFirst()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Peek(i)) return new Cell(_startRow + i / 3, _startCol + i % 3);
        }

        return new Cell(-1, -1);
    }
    
    public Cell Next(ref int cursor)
    {
        cursor++;
        for (; cursor < 9; cursor++)
        {
            if (Peek(cursor)) return new Cell(_startRow + cursor / 3, _startCol + cursor % 3);
        }

        return new Cell(-1, -1);
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Peek(i)) yield return new Cell(_startRow + i / 3, _startCol + i % 3);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public MiniGridPositions Copy()
    {
        return new MiniGridPositions(_pos, Count, _startRow, _startCol);
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

    public void ForEachCombination(IReadOnlyMiniGridPositions.HandleCombination handler)
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

    public int MiniGridNumber()
    {
        return _startRow + _startCol / 3;
    }
    
    public MiniGridPositions Or(IReadOnlyMiniGridPositions pos)
    {
        if (pos is not MiniGridPositions mini) return IReadOnlyMiniGridPositions.DefaultOr(pos, this);
        int newPos = _pos | mini._pos;
        return new MiniGridPositions(newPos, System.Numerics.BitOperations.PopCount((uint)newPos), _startRow,
            _startCol);
    }
    
}