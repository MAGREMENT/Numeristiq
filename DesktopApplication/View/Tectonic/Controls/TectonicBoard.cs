using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using DesktopApplication.Presenter.Tectonic.Solve;

namespace DesktopApplication.View.Tectonic.Controls;

public class TectonicBoard : DrawingBoard, IAddChild, ITectonicDrawer
{
    private const int BackgroundIndex = 0;
    private const int SmallLinesIndex = 1;
    private const int BigLinesIndex = 2;
    private const int NumbersIndex = 3;

    private double _cellSize;
    private int _rowCount;
    private int _columnCount;
    private double _bigLineWidth;
    private double _smallLineWidth;

    public event OnDimensionCountChange? RowCountChanged;
    public event OnDimensionCountChange? ColumnCountChanged;
    
    public static readonly DependencyProperty DefaultNumberBrushProperty =
        DependencyProperty.Register(nameof(DefaultNumberBrush), typeof(Brush), typeof(TectonicBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not TectonicBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(NumbersIndex, brush);
                board.Refresh();
            }));
    
    public Brush DefaultNumberBrush
    {
        set => SetValue(DefaultNumberBrushProperty, value);
        get => (Brush)GetValue(DefaultNumberBrushProperty);
    }
    
    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(nameof(BackgroundBrush), typeof(Brush), typeof(TectonicBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not TectonicBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(BackgroundIndex, brush);
                board.Refresh();
            }));

    public Brush BackgroundBrush
    {
        set => SetValue(BackgroundBrushProperty, value);
        get => (Brush)GetValue(BackgroundBrushProperty);
    }
    
    public static readonly DependencyProperty LineBrushProperty =
        DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(TectonicBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not TectonicBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(SmallLinesIndex, brush);
                board.SetLayerBrush(BigLinesIndex, brush);
                board.Refresh();
            }));
    
    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }

    public double CellSize
    {
        get => _cellSize;
        set
        {
            _cellSize = value;
            UpdateSize();
        }
    }

    public int RowCount
    {
        get => _rowCount;
        set
        {
            var old = _rowCount;
            _rowCount = value;
            UpdateSize();

            if (_rowCount != old) RowCountChanged?.Invoke(_rowCount);
        }
    }

    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            var old = _columnCount;
            _columnCount = value;
            UpdateSize();

            if (_columnCount != old) ColumnCountChanged?.Invoke(_columnCount);
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

    public TectonicBoard() : base(4)
    {
        Borders.CountChanged += UpdateAndDrawLines;
    }
    
    public void AddChild(object value)
    {
        if (value is NeighborBorder border) Borders.Add(border);
    }

    public void AddText(string text)
    {
        
    }

    #region ITectonicDrawer

    public void ClearNumbers()
    {
        Layers[NumbersIndex].Clear();
    }

    public void ShowSolution(int row, int column, int number)
    {
        Dispatcher.Invoke(() =>
        {
            Layers[NumbersIndex].Add(new TextInRectangleComponent(number.ToString(), _cellSize * 3 / 4,
                DefaultNumberBrush, new Rect(GetLeft(column), GetTop(row), _cellSize,
                    _cellSize), ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center));
        });
    }

    public void ShowPossibilities(int row, int column, IEnumerable<int> possibilities, int zoneSize)
    {
        var posSize = _cellSize / zoneSize;
        var textSize = posSize * 3 / 4;
        var delta = (_cellSize - posSize) / 2;
        
        foreach (var possibility in possibilities)
        {
            Dispatcher.Invoke(() =>
            {
                Layers[NumbersIndex].Add(new TextInRectangleComponent(possibility.ToString(), textSize,
                    DefaultNumberBrush, new Rect(GetLeft(column) + (possibility - 1) * posSize, GetTop(row) + delta, posSize,
                        posSize), ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center)); 
            });
        }
    }

    public void ClearBorderDefinitions()
    {
        RefreshAllowed = false;
        Borders.Clear();
        RefreshAllowed = true;
    }

    public void AddBorderDefinition(int insideRow, int insideColumn, BorderDirection direction, bool isThin)
    {
        RefreshAllowed = false;
        Borders.Add(new NeighborBorder(insideRow, insideColumn, direction, isThin));
        RefreshAllowed = true;
    }

    #endregion

    #region Private

    private double GetLeft(int column)
    {
        return _cellSize * column + _bigLineWidth * (column + 1);
    }

    private double GetTop(int row)
    {
        return _cellSize * row + _bigLineWidth * (row + 1);
    }

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
        Refresh();
    }
    
    private void UpdateBackground()
    {
        Layers[BackgroundIndex].Add(new FilledRectangleComponent(
            new Rect(0, 0, Width, Height), BackgroundBrush));
    }

    private void UpdateAndDrawLines()
    {
        Layers[SmallLinesIndex].Clear();
        Layers[BigLinesIndex].Clear();
        UpdateLines();
        Refresh();
    }

    private void UpdateLines()
    {
        if (RowCount == 0 || ColumnCount == 0) return;
        
        var half = _bigLineWidth / 2;
        
        Layers[BigLinesIndex].Add(new OutlinedRectangleComponent(
            new Rect(half, half, Width - half, Height - half), new Pen(LineBrush, _bigLineWidth)));

        var diff = (_bigLineWidth - _smallLineWidth) / 2;
        var length = _cellSize + _bigLineWidth * 2;

        //Horizontal
        double deltaX;
        double deltaY = _cellSize + _bigLineWidth;
        
        for (int row = 0; row < _rowCount - 1; row++)
        {
            deltaX = 0;
            
            for (int col = 0; col < _columnCount; col++)
            {
                var b = Get(BorderDirection.Horizontal, row, col);

                if (b is not null && b.IsThin)
                {
                    Layers[SmallLinesIndex].Add(new FilledRectangleComponent(
                        new Rect(deltaX, deltaY + diff, length, _smallLineWidth), LineBrush));
                }
                else
                {
                    Layers[BigLinesIndex].Add(new FilledRectangleComponent(
                        new Rect(deltaX, deltaY, length, _bigLineWidth), LineBrush));
                }

                deltaX += _cellSize + _bigLineWidth;
            }

            deltaY += _cellSize + _bigLineWidth;
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
                    Layers[SmallLinesIndex].Add(new FilledRectangleComponent(
                        new Rect(deltaX + diff, deltaY, _smallLineWidth, length), LineBrush));
                }
                else
                {
                    Layers[BigLinesIndex].Add(new FilledRectangleComponent(
                        new Rect(deltaX, deltaY, _bigLineWidth, length), LineBrush));
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

    #endregion
}

public record NeighborBorder(int InsideRow, int InsideColumn, BorderDirection Direction, bool IsThin);

public class NotifyingList<T> : IList, IList<T>
{
    private T[] _array = Array.Empty<T>();
    
    public int Count { get; private set; }
    public bool IsSynchronized => false;
    public object SyncRoot => null!;
    public bool IsFixedSize => false;
    public bool IsReadOnly => false;
    
    public event OnCountChange? CountChanged;
    
    public int Add(object? value)
    {
        if (value is not T item) return -1;

        GrowIfNecessary();

        _array[Count++] = item;
        CountChanged?.Invoke();
        return Count - 1;
    }

    public void Add(T item)
    {
        GrowIfNecessary();

        _array[Count++] = item;
        CountChanged?.Invoke();
    }

    public void Clear()
    {
        Count = 0;
        CountChanged?.Invoke();
    }

    public bool Contains(T item)
    {
        for(int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is not null && o.Equals(item)) return true;
        }

        return false;
    }
    
    public bool Contains(object? value)
    {
        for(int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is null)
            {
                if (value is null) return true;
            }else if (o.Equals(value)) return true;
        }

        return false;
    }
    
    public void CopyTo(Array array, int index)
    {
        Array.Copy(_array, 0, array, index, Count);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(_array, 0, array, arrayIndex, Count);
    }

    public int IndexOf(object? value)
    {
        for(int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is null)
            {
                if (value is null) return i;
            }else if (o.Equals(value)) return i;
        }

        return -1;
    }
    
    public int IndexOf(T item)
    {
        for(int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is not null && o.Equals(item)) return i;
        }

        return -1;
    }

    public void Remove(object? value)
    {
        for (int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is null)
            {
                if (value is null)
                {
                    RemoveAt(i);
                    return;
                }
            }
            else if (o.Equals(value))
            {
                RemoveAt(i);
                return;
            }
        }
    }
    
    public bool Remove(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            var o = _array[i];
            if (o is not null && o.Equals(item))
            {
                RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > Count) return;
        
        GrowIfNecessary();
        
        if (index == Count)
        {
            Add(item);
            return;
        }

        Array.Copy(_array, index, _array, index + 1, Count - index);
        _array[index] = item;
        Count++;
        CountChanged?.Invoke();
    }
    
    public void Insert(int index, object? value)
    {
        if (value is not T item) return;

        Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count) return;
        
        Array.Copy(_array, index + 1, _array, index, Count - index - 1);
        Count--;
        CountChanged?.Invoke();
    }

    T IList<T>.this[int index]
    {
        get => _array[index];
        set => _array[index] = value;
    }

    public object? this[int index]
    {
        get => _array[index];
        set
        {
            if(value is T item) _array[index] = item;
        } 
    }
    
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
    
    //Private-----------------------------------------------------------------------------------------------------------

    private void GrowIfNecessary()
    {
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
    }
}

public delegate void OnCountChange();
public delegate void OnDimensionCountChange(int number);