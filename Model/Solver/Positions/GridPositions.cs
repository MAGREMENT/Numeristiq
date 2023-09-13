using System;
using System.Collections;
using System.Collections.Generic;
using Model.Solver.StrategiesUtil;
using Model.StrategiesUtil;

namespace Model.Solver.Positions;

public class GridPositions : IEnumerable<Cell>
{
    private const int FirstLimit = 53;
    private const ulong RowMask = 0x1FF;
    private const ulong FirstColumnMask = 0x201008040201;
    private const ulong SecondColumnMask = 0x40201;
    private const ulong MiniGridMask = 0x1C0E07;

    private ulong _first; //0 to 53 => First 6 boxes
    private ulong _second; // 54 to 80 => Last 3 boxes

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
    
    public void Remove(int row, int col)
    {
        int n = row * 9 + col;
        if(n > FirstLimit) _second &= ~(1ul << (n - FirstLimit - 1));
        else _first &= ~(1ul << n);
    }

    public int RowCount(int row) //TODO use masks
    {
        int result = 0;
        for (int col = 0; col < 9; col++)
        {
            if (Peek(row, col)) result++;
        }

        return result;
    }

    public int ColumnCount(int column) //TODO use masks
    {
        int result = 0;
        for (int row = 0; row < 9; row++)
        {
            if (Peek(row, column)) result++;
        }

        return result;
    }

    public int MiniGridCount(int miniRow, int miniCol) //TODO use masks
    {
        int result = 0;
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                if (Peek(miniRow * 3 + gridRow, miniCol * 3 + gridCol)) result++;
            }
        }

        return result;
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

    public GridPositions And(GridPositions other)
    {
        return new GridPositions(_first & other._first, _second & other._second);
    }

    public LinePositions RowPositions(int row)
    {
        var i = row >= 6 ? (_second >> ((row - 6) * 9)) & RowMask : (_first >> (row * 9)) & RowMask;
        return new LinePositions((int)i, System.Numerics.BitOperations.PopCount(i));
    }

    public LinePositions ColumnPositions(int col)
    {
        var i = _first >> col;
        var j = _second >> col;
        var k = i & 1 | (i & 0x200) >> 8 | (i & 0x40000) >> 16 |
                (i & 0x8000000) >> 24 | (i & 0x1000000000) >> 32 | (i & 0x200000000000) >> 40 |
                (j & 1) << 6 | (j & 0x200) >> 2 | (j & 0x40000) >> 10;
        return new LinePositions((int)k, System.Numerics.BitOperations.PopCount(k));
    }

    public MiniGridPositions MiniGridPositions(int miniRow, int miniCol)
    {
        var i = miniRow == 2 ? _second >> (miniCol * 3) : _first >> (miniRow * 27 + miniCol * 3);
        var j = (i & 0x7) | (i & 0xE00) >> 6 | (i & 0x1C0000) >> 12;
        return new MiniGridPositions((int)j, System.Numerics.BitOperations.PopCount(j), miniRow * 3, miniCol * 3);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_first, _second);
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
                if (Peek(row, col)) result += $"[{row + 1}, {col + 1}] ";
            }
        }

        return result;
    }

    public string ToBitGrid()
    {
        var result = "";
        for (int i = 0; i <= FirstLimit; i++)
        {
            result += (((_first >> i) & 1) > 0 ? "1" : "0") + " ";
            if ((i + 1) % 9 == 0) result += "\n";
        }

        for (int i = 0; i < 80 - FirstLimit; i++)
        {
            result += (((_second >> i) & 1) > 0 ? "1" : "0") + " ";
            if ((i + 1) % 9 == 0) result += "\n";
        }

        return result;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}