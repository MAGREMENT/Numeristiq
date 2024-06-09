using System;
using Model.Utility;

namespace Model.Tectonics.Generator;

public class RandomEmptyTectonicGenerator : IEmptyTectonicGenerator
{
    private const int MinDimensionCount = 2;
    private const int MaxDimensionCount = 10;

    private int _minRowCount = MinDimensionCount;
    private int _minColumnCount = MinDimensionCount;
    private int _maxRowCount = MaxDimensionCount;
    private int _maxColumnCount = MaxDimensionCount;
    private Random _random = new();

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
        var triedOnce = false;

        do
        {
            if (triedOnce) result = result.CopyWithoutDigits();

            for (int r = 0; r < result.RowCount - 1; r++)
            {
                for (int c = 0; c < result.ColumnCount - 1; c++)
                {
                    var current = new Cell(r, c);
                    var zone = result.GetZone(current);
                    if (zone.Count == IZone.MaxCount) continue;

                    if (_random.Next(1) < 1) result.MergeZones(current, new Cell(r + 1, c));
                    if (_random.Next(1) < 1) result.MergeZones(current, new Cell(r, c + 1));
                }
            }

            triedOnce = true;
        } while (BackTracking.Fill(result, new TectonicPossibilitiesGiver(result), 1).Count == 1);

        return result;
    }

    private static int Enclose(int n)
    {
        n = Math.Min(MaxDimensionCount, n);
        return Math.Max(MinDimensionCount, n);
    }
}