using System.Collections;
using System.Collections.Generic;

namespace Model.Positions;

public class LinePositions : IEnumerable<int>
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

    public bool AreAllInSameMiniGrid()
    {
        //111 111 000
        //111 000 111
        //000 111 111
        return Count is < 4 and > 0 && ((_pos & 0x1F8) == 0 || (_pos & 0x1C7) == 0 || (_pos & 0x3F) == 0);
    }

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < 9; i++)
        {
            if(((_pos >> i) & 1) > 0) yield return i;
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

    public LinePositions Mash(LinePositions pos)
    {
        int newPos = _pos | pos._pos;
        return new LinePositions(newPos, System.Numerics.BitOperations.PopCount((uint)newPos));
    }

    public LinePositions Copy()
    {
        return new LinePositions(_pos, Count);
    }
}