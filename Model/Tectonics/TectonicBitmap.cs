using System;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics;

public class TectonicBitmap : InfiniteBitmap
{
    private readonly ulong _3HighMask;
    
    public TectonicBitmap(int rowCount, int columnCount) : base(rowCount, columnCount)
    {
        _3HighMask = 1UL | (1UL << columnCount) | (1UL << (2 * columnCount));
    }

    public bool HasNeighbor(int row, int col)
    {
        var mask = (1UL << col) | (1UL << Math.Max(0, col - 1)) | (1UL << Math.Min(ColumnCount - 1, col + 1));
        var min = Math.Max(0, row - 1);
        var max = Math.Min(RowCount - 1, row + 1);
        for (int r = min; r <= max; r++)
        {
            var entry = r / _rowsPerEntry;
            var offset = (r - entry * _rowsPerEntry) * ColumnCount;

            if ((_bits[entry] & (mask << offset)) > 0) return true;
        }

        return false;
    }

    public void Add3x3(int row, int col)
    {
        var maxEntry = (row + 1) / _rowsPerEntry;
        if (maxEntry == 0)
        {
            Add3x3ToEntry(row, col, 0);
            return;
        }

        var minEntry = (row - 1) / _rowsPerEntry;
        Add3x3ToEntry(row - minEntry * _rowsPerEntry, col, minEntry);
        
        if (maxEntry != minEntry) Add3x3ToEntry(row - maxEntry * _rowsPerEntry, col, maxEntry);
    }

    private void Add3x3ToEntry(int row, int col, int entry)
    {
        var mask = (_3HighMask << col) | (_3HighMask << Math.Max(0, col - 1)) |
                   (_3HighMask << Math.Min(ColumnCount - 1, col + 1));
        if (row <= 0) mask >>= ColumnCount * (1 - row);
        else mask <<= (row - 1) * ColumnCount;
        _bits[entry] |= mask;
    }
}