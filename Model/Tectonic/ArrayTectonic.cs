using System.Collections;
using System.Collections.Generic;
using System.Text;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonic;

public class ArrayTectonic : ITectonic
{
    private readonly TectonicCell[,] _cells;
    private readonly List<IZone> _zones;
    
    public int RowCount => _cells.GetLength(0);
    public int ColumnCount => _cells.GetLength(1);
    public IReadOnlyList<IZone> Zones => _zones;

    public ArrayTectonic(int rowCount, int colCount)
    {
        _cells = new TectonicCell[rowCount, colCount];
        _zones = new List<IZone>();
    }

    public void AddZone(IReadOnlyList<Cell> cells)
    {
        InfiniteBitSet bitSet = new();
        foreach (var cell in cells)
        {
            var n = cell.Row * ColumnCount + cell.Column;
            if (bitSet.IsSet(n)) return;

            bitSet.Set(n);
            if (_cells[cell.Row, cell.Column].Zone is null) return;
        }

        AddZoneUnchecked(cells);
    }

    public void AddZoneUnchecked(IReadOnlyList<Cell> cells)
    {
        var z = new MultiZone(cells, ColumnCount);
        _zones.Add(z);
        foreach (var cell in z)
        {
            _cells[cell.Row, cell.Column].Zone = z;
        }
    }

    public int this[int row, int col] => _cells[row, col].Number;
    
    public void Set(int n, int row, int col)
    {
        _cells[row, col].Number = n;
    }

    public void MergeZones(Cell c1, Cell c2)
    {
        var z1 = _cells[c1.Row, c1.Column].Zone;
        var z2 = _cells[c2.Row, c2.Column].Zone;
        if (z1 is not null && z2 is not null && z1.Equals(z2)) return;

        List<Cell> total = new();
        if (z1 is null) total.Add(c1);
        else
        {
            total.AddRange(z1);
            _zones.Remove(z1);
        }
        if (z2 is null) total.Add(c2);
        else
        {
            total.AddRange(z2);
            _zones.Remove(z2);
        }

        AddZoneUnchecked(total);
    }

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        return new ReadOnlyBitSet16();
    }

    public IZone GetZone(Cell cell)
    {
        return _cells[cell.Row, cell.Column].Zone ?? new SoloZone(cell);
    }

    public bool IsFromSameZone(Cell c1, Cell c2)
    {
        return GetZone(c1).Contains(c2);
    }

    public override string ToString()
    {
        StringBuilder builder = new();

        for (int row = 0; row < RowCount; row++)
        {
            if (row == 0) builder.Append(StringUtility.Repeat("+---", ColumnCount) + "+\n");
            else
            {
                for (int col = 0; col < ColumnCount; col++)
                {
                    builder.Append(IsFromSameZone(new Cell(row, col), new Cell(row - 1, col))
                        ? "+   "
                        : "+---");
                }

                builder.Append("+\n");
            }

            for (int col = 0; col < ColumnCount; col++)
            {
                var c = _cells[row, col].Number;
                var n = c == 0 ? " " : c.ToString();
                if (col == 0 || !IsFromSameZone(new Cell(row, col), new Cell(row, col - 1))) builder.Append($"| {n} ");
                else builder.Append($"  {n} ");
            }

            builder.Append("|\n");
        }
        
        builder.Append(StringUtility.Repeat("+---", ColumnCount) + "+\n");

        return builder.ToString();
    }
}

public struct TectonicCell
{
    public int Number { get; set; }
    public IZone? Zone { get; set; }
}

