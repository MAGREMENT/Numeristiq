namespace Model.Utility.BitSets;

public class CalibratedInfiniteBitMap : InfiniteBitMap
{
    private readonly ulong _columnMask;
    private readonly ulong _rowMask;
    
    public CalibratedInfiniteBitMap(int rowCount, int columnCount) : base(rowCount, columnCount)
    {
        for (int i = 0; i < RowsPerUnit; i++)
        {
            _columnMask |= 1UL << (i * columnCount);
        }

        _rowMask = ~(ulong.MaxValue << columnCount);
    }

    public void FillRow(int row)
    {
        var entry = row / RowsPerUnit;
        _bits[entry] |= _rowMask << ((row - entry * RowsPerUnit) * ColumnCount);
    }

    public void FillColumn(int col)
    {
        for (int i = 0; i < _bits.Length; i++)
        {
            _bits[i] |= _columnMask << col;
        }
    }

    public bool IsRowEmpty(int row)
    {
        var entry = row / RowsPerUnit;
        return (_bits[entry] & (_rowMask << ((row - entry * RowsPerUnit) * ColumnCount))) == 0;
    }

    public bool IsColumnEmpty(int col)
    {
        foreach (var unit in _bits)
        {
            if((unit & (_columnMask << col)) > 0) return false;
        }

        return true;
    }
}