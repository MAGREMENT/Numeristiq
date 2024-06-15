using System;
using System.Collections.Generic;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Tectonics.Generator;

public class RandomLayoutBackTrackingFilledTectonicGenerator : IFilledPuzzleGenerator<ITectonic>
{
    private const int MinDimensionCount = 2;
    private const int MaxDimensionCount = 10;

    private int _minRowCount = MinDimensionCount;
    private int _minColumnCount = MinDimensionCount;
    private int _maxRowCount = MaxDimensionCount;
    private int _maxColumnCount = MaxDimensionCount;
    private readonly Random _random = new();

    public int MinRowCount
    {
        set
        {
            _minRowCount = Enclose(value);
            if (_minRowCount > _maxRowCount) _maxRowCount = _minRowCount;
        }
    }

    public int MinColumnCount
    {
        set
        {
            _minColumnCount = Enclose(value);
            if (_minColumnCount > _maxColumnCount) _maxColumnCount = _minColumnCount;
        }
    }

    public int MaxRowCount
    {
        set
        {
            _maxRowCount = Enclose(value);
            if (_maxRowCount < _minRowCount) _minRowCount = _maxRowCount;
        }
    }

    public int MaxColumnCount
    {
        set
        {
            _maxColumnCount = Enclose(value);
            if (_maxColumnCount < _minColumnCount) _minColumnCount = _maxColumnCount;
        }
    }

    public ITectonic Generate()
    {
        ITectonic result = new ArrayTectonic(_random.Next(_minRowCount, _maxRowCount),
            _random.Next(_minColumnCount, _maxColumnCount));

        while(true)
        {
            if (result.Zones.Count > 0) result.ClearZones();

            for (int r = 0; r < result.RowCount; r++)
            {
                for (int c = 0; c < result.ColumnCount; c++)
                {
                    var current = new Cell(r, c);
                    var zone = result.GetZone(current);
                    if (zone.Count == IZone.MaxCount) continue;

                    if(r < result.RowCount - 1 && _random.Next(2) < 1)
                        result.MergeZones(current, new Cell(r + 1, c));
                    
                    if (c < result.ColumnCount - 1 && _random.Next(2) < 1)
                        result.MergeZones(current, new Cell(r, c + 1));
                    
                    zone = result.GetZone(current);
                    if (zone.Count > 1) continue;

                    if (r > 0)
                    {
                        var cell = new Cell(r - 1, c);
                        if (result.GetZone(cell).Count == 1) result.MergeZones(current, cell);
                    }

                    if (c > 0)
                    {
                        var cell = new Cell(r, c - 1);
                        if (result.GetZone(cell).Count == 1) result.MergeZones(current, cell);
                    }
                }
            }

            var buffer = BackTracking.Solutions(result, new TectonicPossibilitiesGiver(result), 1);
            if (buffer.Count > 0)
            {
                result = buffer[0];
                break;
            }
        }

        return result;
    }
    
    public ITectonic Generate(out List<Cell> removableCells)
    {
        var t = Generate();
        removableCells = GetRemovableCells(t);
        return t;
    }

    private static List<Cell> GetRemovableCells(ITectonic tectonic)
    {
        var list = new List<Cell>(tectonic.RowCount * tectonic.ColumnCount);
        for (int row = 0; row < tectonic.RowCount; row++)
        {
            for (int c = 0; c < tectonic.ColumnCount; c++)
            {
                list.Add(new Cell(row, c));
            }
        }

        return list;
    }

    private static int Enclose(int n)
    {
        n = Math.Min(MaxDimensionCount, n);
        return Math.Max(MinDimensionCount, n);
    }
}