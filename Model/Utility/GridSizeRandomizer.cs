using System;

namespace Model.Utility;

public class GridSizeRandomizer : Randomizer
{
    private readonly int _fixedMin;
    private readonly int _fixedMax;
    
    private int _minRowCount;
    private int _minColumnCount;
    private int _maxRowCount;
    private int _maxColumnCount;
    
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

    public GridSizeRandomizer(int min, int max)
    {
        _fixedMin = min;
        _minRowCount = min;
        _minColumnCount = min;
        _fixedMax = max;
        _maxRowCount = max;
        _maxColumnCount = max;
    }

    public GridSize GenerateSize()
    {
        return new GridSize(_random.Next(_minRowCount, _maxRowCount + 1),
            _random.Next(_minColumnCount, _maxColumnCount));
    }
    
    private int Enclose(int n)
    {
        n = Math.Min(_fixedMax, n);
        return Math.Max(_fixedMin, n);
    }
}

public readonly struct GridSize
{
    public GridSize(int rowCount, int columnCount)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
    }

    public int RowCount { get; }
    public int ColumnCount { get; }
}