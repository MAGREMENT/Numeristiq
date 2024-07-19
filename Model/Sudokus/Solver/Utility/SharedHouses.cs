using System.Collections;
using System.Collections.Generic;
using System.Text;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility;

//TODO merge these 2 classes
public class SharedHouses : IEnumerable<House>
{
    //3 times 4 bits representing the house number and then 1 bit for if it is shared (1) or not (0)
    //1mmmm1cccc1rrrr
    private int _units;
    public int Count { get; private set; }

    private SharedHouses(int row, int col)
    {
        _units = row | 1 << 4 | col << 5 | 1 << 9 | ToGridNumber(row, col) << 10 | 1 << 14;
        Count = 3;
    }

    public SharedHouses(Cell cell) : this(cell.Row, cell.Column)
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

    public IEnumerator<House> GetEnumerator()
    {
        if (Peek(4)) yield return new House(Unit.Row, GetRow());
        if (Peek(9)) yield return new House(Unit.Column, GetColumn());
        if (Peek(14)) yield return new House(Unit.Box, GetGridNumber());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public readonly struct CommonHouses
{
    private const uint RowRemove = ~(uint)0b1000;
    private const uint ColumnRemove = ~(uint)0b100;
    private const uint BoxRemove = ~(uint)0b10;
    
    private readonly uint _sharedHouses;
    private readonly int _row;
    private readonly int _col;

    public CommonHouses(Cell cell)
    {
        _row = cell.Row;
        _col = cell.Column;
        //RowBit - ColumnBit - BoxBit - InitializationBit
        _sharedHouses = 0b1111;
    }

    private CommonHouses(int row, int col, uint sharedHouses)
    {
        _row = row;
        _col = col;
        _sharedHouses = sharedHouses;
    }
    
    public bool IsValid() => _sharedHouses > 1;

    public CommonHouses Adapt(Cell cell)
    {
        if (_sharedHouses == 0) return new CommonHouses(cell);
        
        var sh = _sharedHouses;
        if (_row != cell.Row) sh &= RowRemove;
        if (_col != cell.Column) sh &= ColumnRemove;
        if (_row / 3 != cell.Row / 3 || _col / 3 != cell.Column / 3) sh &= BoxRemove;

        return new CommonHouses(_row, _col, sh);
    }

    public void CurrentlyCoveringHouses(StringBuilder builder)
    {
        bool alreadyOne = false;
        if (((_sharedHouses >> 3) & 1) > 0)
        {
            alreadyOne = true;
            builder.Append("r" + (_row + 1));
        }
        
        if (((_sharedHouses >> 2) & 1) > 0)
        {
            if (alreadyOne) builder.Append(" or ");
            alreadyOne = true;
            builder.Append("c" + (_col + 1));
        }

        if (((_sharedHouses >> 1) & 1) > 0)
        {
            if (alreadyOne) builder.Append(" or ");
            builder.Append("b" + (_row / 3 * 3 + _col / 3));
        }
    }

    public IEnumerable<Cell> SeenCells()
    {
        if (((_sharedHouses >> 3) & 1) > 0)
        {
            for (int c = 0; c < 9; c++)
            {
                yield return new Cell(_row, c);
            }
        }
        
        if (((_sharedHouses >> 2) & 1) > 0)
        {
            for (int r = 0; r < 9; r++)
            {
                yield return new Cell(r, _col);
            }
        }

        if (((_sharedHouses >> 1) & 1) > 0)
        {
            var sr = _row / 3 * 3;
            var sc = _col / 3 * 3;

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    yield return new Cell(sr + r, sc + c);
                }
            }
        }
    }
}