using System.Collections;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Sudokus.Solver.StrategiesUtility;

public class SharedUnits : IEnumerable<SharedUnit>
{
    //3 times 4 bits representing the unit number and then 1 bit for if it is shared (1) or not (0)
    //1mmmm1cccc1rrrr
    private int _units;
    public int Count { get; private set; }

    private SharedUnits(int row, int col)
    {
        _units = row | 1 << 4 | col << 5 | 1 << 9 | ToGridNumber(row, col) << 10 | 1 << 14;
        Count = 3;
    }

    public SharedUnits(Cell cell) : this(cell.Row, cell.Column)
    {
        
    }

    private void Share(int row, int col)
    {
        if (Peek(4))
        {
            if (GetRow() != row)
            {
                Remove(4);
                Count--;
            }
        }

        if (Peek(9))
        {
            if (GetColumn() != col)
            {
                Remove(9);
                Count--;
            }
        }

        if (Peek(14))
        {
            if (GetGridNumber() != ToGridNumber(row, col))
            {
                Remove(14);
                Count--;
            }
        }
    }

    public void Share(Cell cell)
    {
        Share(cell.Row, cell.Column);
    }

    private int ToGridNumber(int row, int col)
    {
        return row / 3 * 3 + col / 3;
    }

    private bool Peek(int i)
    {
        return ((_units >> i) & 1) > 0;
    }

    private void Remove(int i)
    {
        _units &= ~(1 << i);
    }

    private int GetRow()
    {
        return _units & 0xF;
    }

    private int GetColumn()
    {
        return (_units >> 5) & 0xF;
    }

    private int GetGridNumber()
    {
        return (_units >> 10) & 0xF;
    }

    public IEnumerator<SharedUnit> GetEnumerator()
    {
        if (Peek(4)) yield return new SharedUnit(Unit.Row, GetRow());
        if (Peek(9)) yield return new SharedUnit(Unit.Column, GetColumn());
        if (Peek(14)) yield return new SharedUnit(Unit.MiniGrid, GetGridNumber());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public readonly struct SharedUnit
{
    public SharedUnit(Unit unit, int number)
    {
        Unit = unit;
        Number = number;
    }

    public Unit Unit { get; }
    public int Number { get; }
}