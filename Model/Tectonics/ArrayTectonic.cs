﻿using System;
using System.Collections.Generic;
using System.Text;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

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
    
    public int GetSolutionCount()
    {
        int total = 0;
        foreach (var cell in _cells)
        {
            if (cell.Number != 0) total++;
        }

        return total;
    }
    
    public void AddZone(IReadOnlyList<Cell> cells)
    {
        if (cells.Count > IZone.MaxCount) return;
        InfiniteBitSet bitSet = new();
        foreach (var cell in cells)
        {
            var n = cell.Row * ColumnCount + cell.Column;
            if (bitSet.Contains(n)) return;

            bitSet.Add(n);
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
            _cells[cell.Row, cell.Column] += z;
        }
    }

    public void ClearZones()
    {
        _zones.Clear();
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                _cells[row, col] += null;
            }
        }
    }

    public ITectonic Transfer(int rowCount, int columnCount)
    {
        if (rowCount == 0 && columnCount == 0) return new BlankTectonic();

        var result = new ArrayTectonic(rowCount, columnCount);
        if (rowCount == 0 || columnCount == 0) return result;
        
        var minRow = Math.Min(rowCount, RowCount);
        var minCol = Math.Min(columnCount, ColumnCount);
        
        for(int row = 0; row < minRow; row++)
        {
            for (int col = 0; col < minCol; col++)
            {
                result[row, col] = this[row, col];
            }
        }

        foreach (var zone in _zones)
        {
            List<Cell> cellsInbound = new();
            foreach (var cell in zone)
            {
                if (cell.Row < minRow && cell.Column < minCol) cellsInbound.Add(cell);
            }

            result.AddZoneUnchecked(cellsInbound);
        }

        return result;
    }

    public int this[int row, int col]
    {
        get => _cells[row, col].Number;
        set => _cells[row, col] += value;
    }

    public bool MergeZones(Cell c1, Cell c2)
    {
        return MergeZones(GetZone(c1), GetZone(c2));
    }

    public bool MergeZones(IZone z1, IZone z2)
    {
        if (z1.Count + z2.Count > IZone.MaxCount || z1.Equals(z2) ||
            !TectonicUtility.AreAdjacent(z1, z2)) return false;
        
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

    public bool CreateZone(IEnumerable<Cell> cells)
    {
        UniqueList<IZone> zones = new();
        List<Cell> cellsIn = new();

        foreach (var cell in cells)
        {
            zones.Add(GetZone(cell));
            cellsIn.Add(cell);
        }

        if (cellsIn.Count > IZone.MaxCount) return false;

        foreach (var zone in zones)
        {
            List<Cell> notIn = new();
            foreach (var cell in zone)
            {
                if(!cellsIn.Contains(cell)) notIn.Add(cell);
            }

            RemoveZone(zone);
            if(notIn.Count == 0) continue;
            
            foreach (var otherZone in CellUtility.DivideInAdjacentCells(notIn))
            {
                AddZoneUnchecked(otherZone);
                CheckZoneIntegrity(_zones.Count - 1);
            }
        }

        AddZoneUnchecked(cellsIn);
        CheckZoneIntegrity(_zones.Count - 1);

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

    public int GetZoneNumber(IZone zone)
    {
        for (var i = 0; i < _zones.Count; i++)
        {
            if (_zones[i].Equals(zone)) return i;
        }

        return -1;
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

                foreach (var cell in TectonicUtility.GetNeighbors(row, col, RowCount, ColumnCount))
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
                var num = this[cell.Row, cell.Column];
                if (!toCheck.Contains(num)) return false;
                toCheck -= num;
            }

            if (toCheck.Count != 0) return false;
        }

        return true;
    }

    public bool IsComplete()
    {
        foreach (var cell in _cells)
        {
            if (cell.Number == 0) return false;
        }

        return true;
    }

    public ITectonic Copy()
    {
        var result = CopyWithoutDigits();

        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                var number = _cells[row, col].Number;
                if (number != 0) result[row, col] += number;
            }
        }

        return result;
    }

    public ITectonic CopyWithoutDigits()
    {
        var result = new ArrayTectonic(RowCount, ColumnCount);
        foreach (var zone in _zones)
        {
            result.AddZone(zone);
        }

        return result;
    }

    public bool SameDigits(ITectonic tectonic)
    {
        if (tectonic.RowCount != RowCount || tectonic.ColumnCount != ColumnCount) return false;
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                if (tectonic[row, col] != this[row, col]) return false;
            }
        }

        return true;
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
            _cells[cell.Row, cell.Column] += zone;
        }
    }

    private void RemoveZone(IZone zone)
    {
        if (!_zones.Remove(zone)) return;
        
        foreach (var cell in zone)
        {
            _cells[cell.Row, cell.Column] += null;
        }
    }

    private void CheckZoneIntegrity(int zoneIndex)
    {
        var zone = _zones[zoneIndex];
        foreach (var cell in zone)
        {
            if (_cells[cell.Row, cell.Column].Number > zone.Count) _cells[cell.Row, cell.Column] += 0;
        }
    }
}

public readonly struct TectonicCell
{
    public TectonicCell()
    {
        Number = 0;
        Zone = null;
    }

    public TectonicCell(int number, IZone? zone)
    {
        Number = number;
        Zone = zone;
    }
    
    public int Number { get; }
    public IZone? Zone { get; }

    public static TectonicCell operator +(TectonicCell left, int n) => new(n, left.Zone);
    public static TectonicCell operator +(TectonicCell left, IZone? zone) => new(left.Number, zone);
}

