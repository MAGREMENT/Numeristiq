using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using DesktopApplication.Presenter.Tectonic.Solve;
using DesktopApplication.View.Utility;
using Model;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;
using MathUtility = DesktopApplication.View.Utility.MathUtility;

namespace DesktopApplication.View.Tectonic.Controls;

public class TectonicBoard : DrawingBoard, IAddChild, ITectonicDrawer
{
    private const int BackgroundIndex = 0;
    private const int CellHighlightIndex = 1;
    private const int PossibilityHighlightIndex = 2;
    private const int SmallLinesIndex = 3;
    private const int BigLinesIndex = 4;
    private const int NumbersIndex = 5;
    private const int LinksIndex = 6;
    
    private const double LinkOffset = 20;

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

    public static readonly DependencyProperty LinkBrushProperty =
        DependencyProperty.Register(nameof(LinkBrush), typeof(Brush), typeof(TectonicBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not TectonicBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(LinksIndex, brush);
                board.Refresh();
            }));

    public Brush LinkBrush
    {
        set => SetValue(LinkBrushProperty, value);
        get => (Brush)GetValue(LinkBrushProperty);
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
    private readonly CellsAssociations _associatedCells;

    public TectonicBoard() : base(7)
    {
        _associatedCells = new CellsAssociations(RowCount, ColumnCount);
        
        Borders.Cleared += () =>
        {
            _associatedCells.New(RowCount, ColumnCount);
            UpdateAndDrawLines();
        };

        Borders.ElementAdded += e =>
        {
            if (!e.IsThin) return;
            
            var cells = e.ComputeNeighboringCells();
            _associatedCells.Merge(cells.Item1, cells.Item2);
            
            UpdateAndDrawLines();
        };
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

    public void ShowPossibilities(int row, int column, IEnumerable<int> possibilities)
    {
        var zoneSize = _associatedCells.CountAt(row, column);
        var posSize = _cellSize / zoneSize;
        var textSize = posSize * 3 / 4;
        var delta = (_cellSize - posSize) / 2;
        
        foreach (var possibility in possibilities)
        {
            if(possibility > zoneSize) continue;
            
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

    public void ClearHighlights()
    {
        Layers[CellHighlightIndex].Clear();
        Layers[PossibilityHighlightIndex].Clear();
        Layers[LinksIndex].Clear();
    }

    public void FillPossibility(int row, int col, int possibility, ChangeColoration coloration)
    {
        var size = GetPossibilitySize(row, col);
        var delta = (_cellSize - size) / 2;
        
        Layers[PossibilityHighlightIndex].Add(new FilledRectangleComponent(new Rect(GetLeft(col) + (possibility - 1) * size, GetTop(row) + delta,
            size, size), new SolidColorBrush(ColorUtility.ToColor(coloration))));
    }

    public void FillCell(int row, int col, ChangeColoration coloration)
    {
        Layers[CellHighlightIndex].Add(new FilledRectangleComponent(new Rect(GetLeft(col), GetTop(row),
            _cellSize, _cellSize), new SolidColorBrush(ColorUtility.ToColor(coloration))));
    }
    
    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority)
    {
        var from = Center(rowFrom, colFrom, possibilityFrom);
        var to = Center(rowTo, colTo, possibilityTo);
        var middle = new Point(from.X + (to.X - from.X) / 2, from.Y + (to.Y - from.Y) / 2);

        var offsets = MathUtility.ShiftSecondPointPerpendicularly(from, middle, LinkOffset);

        var validOffsets = new List<Point>();
        for (int i = 0; i < 2; i++)
        {
            var p = offsets[i];
            if(p.X > 0 && p.X < Width && p.Y > 0 && p.Y < Height) validOffsets.Add(p);
        }

        bool isWeak = strength == LinkStrength.Weak;
        var fromSize = GetPossibilitySize(rowFrom, colFrom);
        var toSize = GetPossibilitySize(rowTo, colTo);
        switch (validOffsets.Count)
        {
            case 0 : 
                AddShortenedLine(from, fromSize, to, toSize, isWeak);
                break;
            case 1 :
                AddShortenedLine(from, fromSize, validOffsets[0], to, toSize, isWeak);
                break;
            case 2 :
                if(priority == LinkOffsetSidePriority.Any) 
                    AddShortenedLine(from, fromSize, validOffsets[0], to, toSize, isWeak);
                else
                {
                    var left = MathUtility.IsLeft(from, to, validOffsets[0]) ? 0 : 1;
                    AddShortenedLine(from, fromSize, priority == LinkOffsetSidePriority.Left 
                        ? validOffsets[left] 
                        : validOffsets[(left + 1) % 2], to, toSize, isWeak);
                }
                break;
        }
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

    private Point Center(int row, int col, int possibility)
    {
        var size = GetPossibilitySize(row, col);
        var delta = (_cellSize - size) / 2;

        return new Point(GetLeft(col) + size * (possibility -1) + size / 2, GetTop(row) + delta + size / 2);
    }

    private double GetPossibilitySize(int row, int col)
    {
        var zoneSize = _associatedCells.CountAt(row, col);
        return _cellSize / zoneSize;
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
    
    private void AddShortenedLine(Point from, double fromSize, Point to, double toSize, bool isWeak)
    {
        var fromShortening = fromSize / 2;
        var toShortening = toSize / 2;

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var mag = Math.Sqrt(dx * dx + dy * dy);
        var newFrom = new Point(from.X + fromShortening * dx / mag, from.Y + fromShortening * dy / mag);
        var newTo = new Point(to.X - toShortening * dx / mag, to.Y - toShortening * dy / mag);
        
        AddLine(newFrom, newTo, isWeak);
    }
    
    private void AddShortenedLine(Point from, double fromSize, Point middle, Point to, double toSize, bool isWeak)
    {
        var fromShortening = fromSize / 2;
        var toShortening = toSize / 2;
        
        var dxFrom = middle.X - from.X;
        var dyFrom = middle.Y - from.Y;
        var mag = Math.Sqrt(dxFrom * dxFrom + dyFrom * dyFrom);
        var newFrom = new Point(from.X +fromShortening * dxFrom / mag, from.Y + fromShortening * dyFrom / mag);

        var dxTo = to.X - middle.X;
        var dyTo = to.Y - middle.Y;
        mag = Math.Sqrt(dxTo * dxTo + dyTo * dyTo);
        var newTo = new Point(to.X - toShortening * dxTo / mag, to.Y - toShortening * dyTo / mag);
            
        AddLine(newFrom, middle, isWeak);
        AddLine(middle, newTo, isWeak);
    }

    private void AddLine(Point from, Point to, bool isWeak)
    {
        Layers[LinksIndex].Add(new LineComponent(from, to, new Pen(LinkBrush, 2)
        {
            DashStyle = isWeak ? DashStyles.DashDot : DashStyles.Solid
        }));
    }

    #endregion
}

public record NeighborBorder(int InsideRow, int InsideColumn, BorderDirection Direction, bool IsThin)
{
    public (Cell, Cell) ComputeNeighboringCells()
    {
        return Direction == BorderDirection.Horizontal 
            ? (new Cell(InsideRow, InsideColumn), new Cell(InsideRow + 1, InsideColumn)) 
            : (new Cell(InsideRow, InsideColumn), new Cell(InsideRow, InsideColumn + 1));
    }
}

public class NotifyingList<T> : IList, IList<T>
{
    private T[] _array = Array.Empty<T>();
    
    public int Count { get; private set; }
    public bool IsSynchronized => false;
    public object SyncRoot => null!;
    public bool IsFixedSize => false;
    public bool IsReadOnly => false;
    
    public event OnClear? Cleared;
    public event OnElementAdded<T>? ElementAdded; 
    
    public int Add(object? value)
    {
        if (value is not T item) return -1;

        Add(item);
        
        return Count - 1;
    }

    public void Add(T item)
    {
        GrowIfNecessary();

        _array[Count++] = item;
        ElementAdded?.Invoke(item);
    }

    public void Clear()
    {
        Count = 0;
        Cleared?.Invoke();
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
        ElementAdded?.Invoke(item);
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

public delegate void OnClear();
public delegate void OnElementAdded<T>(T element);
public delegate void OnDimensionCountChange(int number);