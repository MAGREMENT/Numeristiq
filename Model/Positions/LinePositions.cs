using System.Collections;
using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Positions;

public class LinePositions : IReadOnlyLinePositions
{
    private int _pos;
    public int Count { private set; get; }

    public LinePositions(){}

    private LinePositions(int pos, int count)
    {
        _pos = pos;
        Count = count;
    }

    public void Add(int pos)
    {
        if (!Peek(pos)) Count++;
        _pos |= 1 << pos;
    }

    public bool Peek(int pos)
    {
        return ((_pos >> pos) & 1) > 0;
    }

    public void Remove(int pos)
    {
        bool old = Peek(pos);
        _pos &= ~(1 << pos);
        if (old) Count--;
    }
    
    public void Void()
    {
        _pos = 0;
        Count = 0;
    }

    public bool AreAllInSameMiniGrid()
    {
        //111 111 000
        //111 000 111
        //000 111 111
        return Count is < 4 and > 0 && ((_pos & 0x1F8) == 0 || (_pos & 0x1C7) == 0 || (_pos & 0x3F) == 0);
    }
    
    public int GetFirst()
    {
        for (int i = 0; i < 9; i++)
        {
            if(Peek(i)) return i;
        }

        return -1;
    }

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < 9; i++)
        {
            if(Peek(i)) yield return i;
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not LinePositions pos) return false;
        return _pos == pos._pos;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _pos;
    }

    public override string ToString()
    {
        string result = "";
        for (int i = 0; i < 9; i++)
        {
            if (Peek(i)) result += i + " ";
        }

        return result;
    }

    public LinePositions Or(LinePositions pos)
    {
        int newPos = _pos | pos._pos;
        return new LinePositions(newPos, System.Numerics.BitOperations.PopCount((uint)newPos));
    }

    public LinePositions Copy()
    {
        return new LinePositions(_pos, Count);
    }

    public delegate void HandleCombination(int one, int two);

    public void ForEachCombination(HandleCombination handler)
    {
        for (int i = 0; i < 9; i++)
        {
            if (((_pos >> i) & 1) == 0) continue;
            for (int j = i + 1; j < 9; j++)
            {
                if (((_pos >> j) & 1) == 0) continue;
                handler(i, j);
            }
        }
    }
}