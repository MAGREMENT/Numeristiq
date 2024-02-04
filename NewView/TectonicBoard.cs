using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace NewView;

public class TectonicBoard : DrawingBoard, IAddChild
{
    private const int BackgroundIndex = 0;
    private const int SmallLineIndex = 1;
    private const int BigLineIndex = 2;

    private double _cellSize;
    private double _rowCount;
    private double _columnCount;
    private double _bigLineWidth;
    private double _smallLineWidth;

    public double CellSize
    {
        get => _cellSize;
        set
        {
            _cellSize = value;
            UpdateSize();
        }
    }

    public double RowCount
    {
        get => _rowCount;
        set
        {
            _rowCount = value;
            UpdateSize();
        }
    }

    public double ColumnCount
    {
        get => _columnCount;
        set
        {
            _columnCount = value;
            UpdateSize();
        }
    }

    public double BigLineWidth
    {
        get => _bigLineWidth;
        set
        {
            _bigLineWidth = value;
            UpdateSize();
        }
    }

    public double SmallLineWidth
    {
        get => _smallLineWidth;
        set
        {
            _smallLineWidth = value;
            UpdateSize();
        }
    }

    public NotifyingList<NeighborBorder> Borders { get; } = new();

    public TectonicBoard() : base(3)
    {
        Borders.ElementAdded += UpdateAndDrawLines;
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private void UpdateSize()
    {
        double w = _cellSize * _columnCount + _bigLineWidth * (_columnCount + 1);
        double h = _cellSize * _rowCount + _bigLineWidth * (_rowCount + 1);

        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        
        Clear();
        UpdateBackground();
        UpdateLines();
        Draw();
    }
    
    private void UpdateBackground()
    {
        Layers[BackgroundIndex].Add(new FilledRectangleComponent(
            new Rect(0, 0, Width, Height), Brushes.White));
    }

    private void UpdateAndDrawLines()
    {
        Layers[SmallLineIndex].Clear();
        Layers[BigLineIndex].Clear();
        UpdateLines();
        Draw();
    }

    private void UpdateLines()
    {
        var half = _bigLineWidth / 2;
        
        Layers[BigLineIndex].Add(new OutlinedRectangleComponent(
            new Rect(half, half, Width - half, Height - half), new Pen(Brushes.Black, _bigLineWidth)));

        var diff = (_bigLineWidth - _smallLineWidth) / 2;
        var length = _cellSize + _bigLineWidth * 2;

        //Horizontal
        double deltaX = 0;
        double deltaY;
        
        for (int row = 0; row < _rowCount - 1; row++)
        {
            deltaY = _cellSize + _bigLineWidth;
            
            for (int col = 0; col < _columnCount; col++)
            {
                var b = Get(BorderDirection.Horizontal, row, col);

                if (b is not null && b.IsThin)
                {
                    Layers[SmallLineIndex].Add(new FilledRectangleComponent(
                        new Rect(deltaX, deltaY + diff, length, _smallLineWidth), Brushes.Black));
                }
                else
                {
                    Layers[BigLineIndex].Add(new FilledRectangleComponent(
                        new Rect(deltaX, deltaY, length, _bigLineWidth), Brushes.Black));
                }

                deltaY += _cellSize + _bigLineWidth;
            }

            deltaX += _cellSize + _bigLineWidth;
        }
        
        //Vertical
        deltaY = 0;
        
        for (int row = 0; row < _rowCount; row++)
        {
            deltaX = _cellSize + _bigLineWidth;
            
            for (int col = 0; col < _columnCount - 1; col++)
            {
                var b = Get(BorderDirection.Vertical, row, col);

                if (b is not null && b.IsThin)
                {
                    Layers[SmallLineIndex].Add(new FilledRectangleComponent(
                        new Rect(deltaX + diff, deltaY, _smallLineWidth, length), Brushes.Black));
                }
                else
                {
                    Layers[BigLineIndex].Add(new FilledRectangleComponent(
                        new Rect(deltaX, deltaY, _bigLineWidth, length), Brushes.Black));
                }

                deltaX += _cellSize + _bigLineWidth;
            }

            deltaY += _cellSize + _bigLineWidth;
        }
    }

    private NeighborBorder? Get(BorderDirection direction, int row, int col)
    {
        foreach (var item in Borders)
        {
            if (item is not NeighborBorder nb) continue;
            if (nb.Direction == direction && nb.InsideRow == row && nb.InsideColumn == col) return nb;
        }

        return null;
    }

    public void AddChild(object value)
    {
        if (value is NeighborBorder border) Borders.Add(border);
    }

    public void AddText(string text)
    {
       
    }
}

public class NeighborBorder
{
    public BorderDirection Direction { get; set; } = BorderDirection.Horizontal;
    public int InsideRow { get; set; }
    public int InsideColumn { get; set; }
    public bool IsThin { get; set; }
}

public enum BorderDirection
{
    Horizontal, Vertical
}

public class NotifyingList<T> : IList, IList<T>
{
    private T[] _array = Array.Empty<T>();
    
    public event OnElementAddition? ElementAdded;

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _array[i];
        }
    }

    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _array[i];
        }
    }

    public void CopyTo(Array array, int index)
    {
        Array.Copy(_array, 0, array, index, Count);
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public int Count { get; private set; }
    public bool IsSynchronized => false;
    public object SyncRoot => null!;
    public int Add(object? value)
    {
        if (value is not T item) return -1;

        if (_array.Length <= Count)
        {
            if (_array.Length == 0)
            {
                _array = new T[4];
            }
            else
            {
                var buffer = new T[_array.Length * 2];
                Array.Copy(_array, 0, buffer, 0, _array.Length);
                _array = buffer;
            }
        }

        _array[Count++] = item;
        ElementAdded?.Invoke();
        return Count - 1;
    }

    public void Add(T item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Contains(object? value)
    {
        throw new NotImplementedException();
    }

    public int IndexOf(object? value)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, object? value)
    {
        throw new NotImplementedException();
    }

    public void Remove(object? value)
    {
        throw new NotImplementedException();
    }

    public int IndexOf(T item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, T item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    T IList<T>.this[int index]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public bool IsFixedSize => false;
    public bool IsReadOnly => false;

    public object? this[int index]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}

public delegate void OnElementAddition();