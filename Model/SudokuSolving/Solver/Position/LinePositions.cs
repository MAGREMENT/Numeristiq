using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Global;

namespace Model.SudokuSolving.Solver.Position;

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

    public static LinePositions Filled()
    {
        return new LinePositions(0x1FF, 9);
    }

    public static LinePositions FromBits(int bits)
    {
        return new LinePositions(bits, System.Numerics.BitOperations.PopCount((uint)bits));
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

    public void VoidMiniGrid(int miniUnit)
    {
        _pos &= ~(0b111 << (miniUnit * 3));
        Count = System.Numerics.BitOperations.PopCount((uint)_pos);
    }

    public void Fill()
    {
        _pos = 0x1FF;
        Count = 9;
    }

    public void FillMiniGrid(int miniUnit)
    {
        _pos |= 0b111 << (miniUnit * 3);
        Count = System.Numerics.BitOperations.PopCount((uint)_pos);
    }

    public LinePositions And(IReadOnlyLinePositions pos)
    {
        if (pos is LinePositions lp)
        {
            var and = lp._pos & _pos;
            return new LinePositions(and, System.Numerics.BitOperations.PopCount((uint)and));
        }

        return IReadOnlyLinePositions.DefaultAnd(this, pos);
    }

    public LinePositions Difference(IReadOnlyLinePositions pos)
    {
        if (pos is LinePositions lp)
        {
            var diff = _pos &= ~lp._pos;
            return new LinePositions(diff, System.Numerics.BitOperations.PopCount((uint)diff));
        }

        return IReadOnlyLinePositions.DefaultDifference(this, pos);
    }

    public bool AreAllInSameMiniGrid()
    {
        //111 111 000
        //111 000 111
        //000 111 111
        return Count is < 4 and > 0 && ((_pos & 0x1F8) == 0 || (_pos & 0x1C7) == 0 || (_pos & 0x3F) == 0);
    }

    public int MiniGridCount()
    {
        var count = 0;
        if ((_pos & 0x7) > 1) count++;
        if ((_pos & 0x38) > 1) count++;
        if ((_pos & 0x1B0) > 1) count++;
        
        return count;
    }

    public int First()
    {
        for (int i = 0; i < 9; i++)
        {
            if(Peek(i)) return i;
        }

        return -1;
    }

    public int First(int except)
    {
        for (int i = 0; i < 9; i++)
        {
            if (i == except) continue;
            
            if(Peek(i)) return i;
        }

        return -1;
    }

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Peek(i)) yield return i;
        }
    }

    public int Next(ref int cursor)
    {
        cursor++;
        for (; cursor < 9; cursor++)
        {
            if (Peek(cursor)) return cursor;
        }

        return -1;
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

    public LinePositions Or(IReadOnlyLinePositions pos)
    {
        if (pos is not LinePositions line) return IReadOnlyLinePositions.DefaultOr(this, pos);
        int newPos = _pos | line._pos;
        return new LinePositions(newPos, System.Numerics.BitOperations.PopCount((uint)newPos));
    }

    public LinePositions Copy()
    {
        return new LinePositions(_pos, Count);
    }

    public string ToString(Unit unit, int unitNumber)
    {
        var builder = new StringBuilder();
        
        foreach (var other in this)
        {
            switch (unit)
            {
                case Unit.Row :
                    builder.Append(new Cell(unitNumber, other) + " ");
                    break;
                case Unit.Column :
                    builder.Append(new Cell(other, unitNumber) + " ");
                    break;
            }
        }

        return builder.ToString();
    }

    public Cell[] ToCellArray(Unit unit, int unitNumber)
    {
        var result = new Cell[Count];
        int cursor = 0;
        foreach (var other in this)
        {
            result[cursor] = unit switch
            {
                Unit.Row => new Cell(unitNumber, other),
                Unit.Column => new Cell(other, unitNumber),
                _ => throw new ArgumentException("Unit has to be row or column")
            };

            cursor++;
        }

        return result;
    }
}

