using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonic;

public class ArrayTectonic : ITectonic
{
    private readonly ITectonicCell?[,] _cells;

    public ArrayTectonic(int rowCount, int colCount, IReadOnlyList<IZone> zones)
    {
        _cells = new ITectonicCell?[rowCount, colCount];
        Zones = zones;

        for (int i = 0; i < zones.Count; i++)
        {
            foreach (var cell in zones[i])
            {
                _cells[cell.Row, cell.Column] = new MultiZoneTectonicCell(i);
            }
        }
    }

    public ArrayTectonic(int rowCount, int colCount)
    {
        _cells = new ITectonicCell?[rowCount, colCount];
        Zones = Array.Empty<IZone>();
    }

    public int RowCount => _cells.GetLength(0);
    public int ColumnCount => _cells.GetLength(1);
    public IReadOnlyList<IZone> Zones { get; }

    public int this[int row, int col] => _cells[row, col]?.Number ?? 0;
    
    public void Set(int n, int row, int col)
    {
        _cells[row, col] ??= new SelfZoneTectonicCell(new Cell(row, col));
        _cells[row, col]!.Number = n;
    }

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        return new ReadOnlyBitSet16();
    }

    public IZone GetZone(Cell cell)
    {
        _cells[cell.Row, cell.Column] ??= new SelfZoneTectonicCell(cell);
        return _cells[cell.Row, cell.Column]!.GetZone(Zones);
    }

    public bool ShareAZone(Cell c1, Cell c2)
    {
        return GetZone(c1).Contains(c2);
    }

    public IEnumerable<Cell> EachCell()
    {
        for (int row = 0; row < _cells.GetLength(0); row++)
        {
            for (int col = 0; col < _cells.GetLength(1); col++)
            {
                if (_cells[row, col] is not null) yield return new Cell(row, col);
            }
        }
    }

    public IEnumerable<CellNumber> EachCellNumber()
    {
        for (int row = 0; row < _cells.GetLength(0); row++)
        {
            for (int col = 0; col < _cells.GetLength(1); col++)
            {
                if (_cells[row, col] is not null)
                    yield return new CellNumber(new Cell(row, col), _cells[row, col]!.Number);
            }
        }
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
                    builder.Append(ShareAZone(new Cell(row, col), new Cell(row - 1, col))
                        ? "+   "
                        : "+---");
                }

                builder.Append("+\n");
            }

            for (int col = 0; col < ColumnCount; col++)
            {
                var c = _cells[row, col];
                var n = c is null || c.Number == 0 ? " " : c.Number.ToString();
                if (col == 0 || !ShareAZone(new Cell(row, col), new Cell(row, col - 1))) builder.Append($"| {n} ");
                else builder.Append($"  {n} ");
            }

            builder.Append("|\n");
        }
        
        builder.Append(StringUtility.Repeat("+---", ColumnCount) + "+\n");

        return builder.ToString();
    }
}

public interface ITectonicCell
{
    int Number { get; set; }

    IZone GetZone(IReadOnlyList<IZone> tectonicZones);
}

public class MultiZoneTectonicCell : ITectonicCell
{
    public int Number { get; set; }

    private readonly int _zone;

    public MultiZoneTectonicCell(int zone)
    {
        _zone = zone;
    }
    
    public IZone GetZone(IReadOnlyList<IZone> tectonicZones)
    {
        return tectonicZones[_zone];
    }
}

public class SelfZoneTectonicCell : ITectonicCell, IZone
{
    public int Number { get; set; }
    public int Count => 1;

    private readonly Cell _cell;

    public SelfZoneTectonicCell(Cell cell)
    {
        _cell = cell;
    }

    public IZone GetZone(IReadOnlyList<IZone> tectonicZones)
    {
        return this;
    }

    public IEnumerator<Cell> GetEnumerator()
    {
        yield return _cell;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Cell this[int index] => _cell;

    public bool Contains(Cell c) => c == _cell;

    public override bool Equals(object? obj)
    {
        return obj is IZone { Count: 1 } z && z.Contains(_cell);
    }

    public override int GetHashCode()
    {
        return _cell.GetHashCode();
    }
}