using System.Collections;
using System.Collections.Generic;
using System.Text;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility;

public readonly struct CommonHouses : IEnumerable<House>
{
    private const uint ReverseRowBit = ~(uint)0b1000;
    private const uint ReverseColumnBit = ~(uint)0b100;
    private const uint ReverseBoxBit = ~(uint)0b10;

    private const uint RowBit = 0b1000;
    private const uint ColumnBit = 0b100;
    private const uint BoxBit = 0b10;
    
    private readonly uint _sharedHouses;
    private readonly int _row;
    private readonly int _col;

    public CommonHouses(Cell cell)
    {
        _row = cell.Row;
        _col = cell.Column;
        //RowBit | ColumnBit | BoxBit | InitializationBit
        _sharedHouses = 0b1111;
    }

    private CommonHouses(int row, int col, uint sharedHouses)
    {
        _row = row;
        _col = col;
        _sharedHouses = sharedHouses;
    }
    
    public bool IsValid() => _sharedHouses > 1;
    public bool IsInitialized => (_sharedHouses & 1) > 0;

    public CommonHouses Adapt(Cell cell)
    {
        if (_sharedHouses == 0) return new CommonHouses(cell);
        
        var sh = _sharedHouses;
        if (_row != cell.Row) sh &= ReverseRowBit;
        if (_col != cell.Column) sh &= ReverseColumnBit;
        if (_row / 3 != cell.Row / 3 || _col / 3 != cell.Column / 3) sh &= ReverseBoxBit;

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
    
    public IEnumerator<House> GetEnumerator()
    {
        if ((_sharedHouses & RowBit) > 0) yield return new House(Unit.Row, _row);
        if ((_sharedHouses & ColumnBit) > 0) yield return new House(Unit.Column, _col);
        if ((_sharedHouses & BoxBit) > 0) yield return new House(Unit.Box, _row * 3 + _col);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}