using System.Collections;
using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Positions;

public class MiniGridPositions : IEnumerable<int[]>
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

    public void Add(int gridRow, int gridCol)
    {
        if (!PeekFromGridPositions(gridRow, gridCol)) Count++;
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

    public bool PeekFromGridPositions(int gridRow, int gridCol)
    {
        return ((_pos >> (gridRow * 3 + gridCol)) & 1) > 0;
    }

    public bool PeekFromGridNumber(int gridNumber)
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
        var result = "";
        for (int i = 0; i < 9; i++)
        {
            if (PeekFromGridNumber(i)) result += (_startRow + i / 3) + ", " + (_startCol + i % 3) + " ";
        }

        return result;
    }

    public IEnumerator<int[]> GetEnumerator()
    {
        for (int i = 0; i < 9; i++)
        {
            if(((_pos >> i) & 1) > 0) yield return new[] {_startRow + i / 3, _startCol + i % 3};
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

    public delegate void HandleCombination(Coordinate one, Coordinate two);

    public void ForEachCombination(HandleCombination handler)
    {
        for (int i = 0; i < 9; i++)
        {
            if (((_pos >> i) & 1) == 0) continue;
            for (int j = i + 1; j < 9; j++)
            {
                if (((_pos >> j) & 1) == 0) continue;
                handler(new Coordinate(
                    _startRow + i / 3, _startCol + i % 3),
                    new Coordinate(_startRow + j / 3, _startCol + j % 3));
            }
        }
    }
}