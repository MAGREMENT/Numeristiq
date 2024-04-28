using System.Collections.Generic;
using System.Text;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics;

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
        if (cells.Count > IZone.MaxCount) return;
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

    public int this[int row, int col]
    {
        get => _cells[row, col].Number;
        set => _cells[row, col].Number = value;
    }

    public bool MergeZones(Cell c1, Cell c2)
    {
        return MergeZones(GetZone(c1), GetZone(c2));
    }

    public bool MergeZones(IZone z1, IZone z2)
    {
        if (z1.Count + z2.Count > IZone.MaxCount || z1.Equals(z2) ||
            !TectonicCellUtility.AreAdjacent(z1, z2)) return false;
        
        List<Cell> total = new();
        if (z1 is SoloZone sz1) total.Add(sz1.Cell);
        else
        {
            total.AddRange(z1);
            _zones.Remove(z1);
        }
        if (z2 is SoloZone sz2) total.Add(sz2.Cell);
        else
        {
            total.AddRange(z2);
            _zones.Remove(z2);
        }

        AddZoneUnchecked(total);
        return true;
    }

    public bool SplitZone(IEnumerable<Cell> cells)
    {
        IZone? zone = null;
        List<Cell> first = new();
        
        foreach (var cell in cells)
        {
            if (zone is null) zone = GetZone(cell);
            else if (!zone.Contains(cell)) return false;

            first.Add(cell);
        }

        if (zone is null) return false;

        List<Cell> second = new();
        foreach (var cell in zone)
        {
            if (!first.Contains(cell)) second.Add(cell);
        }

        RemoveZone(zone);
        AddZoneUnchecked(first);
        CheckZoneIntegrity(_zones.Count - 1);

        foreach (var otherZone in TectonicCellUtility.DivideInAdjacentCells(second))
        {
            AddZoneUnchecked(otherZone);
            CheckZoneIntegrity(_zones.Count - 1);
        }

        return true;
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

    public bool IsCorrect()
    {
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                var n = this[row, col];
                if (n == 0) return false;

                foreach (var cell in TectonicCellUtility.GetNeighbors(row, col, RowCount, ColumnCount))
                {
                    if (this[cell.Row, cell.Column] == n) return false;
                }
            }
        }

        foreach (var zone in _zones)
        {
            var toCheck = ReadOnlyBitSet16.Filled(1, zone.Count);
            foreach (var cell in zone)
            {
                toCheck -= this[cell.Row, cell.Column];
            }

            if (toCheck.Count != 0) return false;
        }

        return true;
    }

    public ITectonic Copy()
    {
        var result = new ArrayTectonic(RowCount, ColumnCount);
        foreach (var zone in _zones)
        {
            result.AddZone(zone);
        }

        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                var number = _cells[row, col].Number;
                if (number != 0) _cells[row, col].Number = number;
            }
        }

        return result;
    }

    public override string ToString()
    {
        StringBuilder builder = new();

        for (int row = 0; row < RowCount; row++)
        {
            if (row == 0) builder.Append("+---".Repeat(ColumnCount) + "+\n");
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
        
        builder.Append("+---".Repeat(ColumnCount) + "+\n");

        return builder.ToString();
    }

    private void AddZone(IZone zone)
    {
        _zones.Add(zone);
        foreach (var cell in zone)
        {
            _cells[cell.Row, cell.Column].Zone = zone;
        }
    }

    private void RemoveZone(IZone zone)
    {
        if (!_zones.Remove(zone)) return;
        
        foreach (var cell in zone)
        {
            _cells[cell.Row, cell.Column].Zone = null;
        }
    }

    private void CheckZoneIntegrity(int zoneIndex)
    {
        var zone = _zones[zoneIndex];
        foreach (var cell in zone)
        {
            if(_cells[cell.Row, cell.Column].Number > zone.Count) _cells[cell.Row, cell.Column].Number = 0;
        }
    }
}

public struct TectonicCell
{
    public int Number { get; set; }
    public IZone? Zone { get; set; }
}

