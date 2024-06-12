using System.Text;

namespace Model.Utility.BitSets;

public class InfiniteBitmap
{
    private const int BitsPerEntry = 64;
    
    protected readonly ulong[] _bits;
    protected readonly int _rowsPerEntry;
    
    public int ColumnCount { get; }
    public int RowCount { get; }
    
    public InfiniteBitmap(int rowCount, int columnCount)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
        _rowsPerEntry = BitsPerEntry / ColumnCount;
        _bits = new ulong[RowCount / _rowsPerEntry + 1];
    }

    public bool Contains(int row, int col)
    {
        var entry = row / _rowsPerEntry;
        var offset = (row - entry * _rowsPerEntry) * ColumnCount + col;
        
        return ((_bits[entry] >> offset) & 1) > 0;
    }

    public void Add(int row, int col)
    {
        var entry = row / _rowsPerEntry;
        var offset = (row - entry * _rowsPerEntry) * ColumnCount + col;
        
        _bits[entry] |= 1UL << offset;
    }

    public void Remove(int row, int col)
    {
        var entry = row / _rowsPerEntry;
        var offset = (row - entry * _rowsPerEntry) * ColumnCount + col;
        
        _bits[entry] &= ~(1UL << offset);
    }

    public void Clear()
    {
        for (int i = 0; i < _bits.Length; i++)
        {
            _bits[i] = 0UL;
        }
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        for (int r = 0; r < RowCount; r++)
        {
            for (int c = 0; c < ColumnCount; c++)
            {
                builder.Append(Contains(r, c) ? "1 " : "0 ");
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }
}