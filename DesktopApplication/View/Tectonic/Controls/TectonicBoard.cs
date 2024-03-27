using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using DesktopApplication.Presenter.Tectonic.Solve;
using DesktopApplication.View.Utility;
using Model;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Tectonic;
using Model.Utility;
using MathUtility = DesktopApplication.View.Utility.MathUtility;

namespace DesktopApplication.View.Tectonic.Controls;

public class TectonicBoard : DrawingBoard, IAddChild, ITectonicDrawer
{
    private const int BackgroundIndex = 0;
    private const int CellHighlightIndex = 1;
    private const int PossibilityHighlightIndex = 2;
    private const int CursorIndex = 3;
    private const int SmallLinesIndex = 4;
    private const int BigLinesIndex = 5;
    private const int NumbersIndex = 6;
    private const int LinksIndex = 7;
    
    private const double LinkOffset = 20;
    private const double CursorWidth = 3;

    private double _cellSize;
    private int _rowCount;
    private int _columnCount;
    private double _bigLineWidth;
    private double _smallLineWidth;

    private bool[,] _isSpecialNumberBrush = new bool[0, 0];

    public event OnDimensionCountChange? RowCountChanged;
    public event OnDimensionCountChange? ColumnCountChanged;
    
    public static readonly DependencyProperty DefaultNumberBrushProperty =
        DependencyProperty.Register(nameof(DefaultNumberBrush), typeof(Brush), typeof(TectonicBoard),
            new PropertyMetadata((obj, _) =>
            {
                if(obj is not TectonicBoard board) return;
                board.ReEvaluateNumberBrushes();
                board.Refresh();
            }));
    
    public Brush DefaultNumberBrush
    {
        set => SetValue(DefaultNumberBrushProperty, value);
        get => (Brush)GetValue(DefaultNumberBrushProperty);
    }

    public static readonly DependencyProperty SpecialNumberBrushProperty =
        DependencyProperty.Register(nameof(SpecialNumberBrush), typeof(Brush), typeof(TectonicBoard),
            new PropertyMetadata((obj, _) =>
            {
                if(obj is not TectonicBoard board) return;
                board.ReEvaluateNumberBrushes();
                board.Refresh();
            }));

    public Brush SpecialNumberBrush
    {
        get => (Brush)GetValue(SpecialNumberBrushProperty);
        set => SetValue(SpecialNumberBrushProperty, value);
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
    
    public static readonly DependencyProperty CursorBrushProperty =
        DependencyProperty.Register(nameof(CursorBrush), typeof(Brush), typeof(TectonicBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not TectonicBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(CursorIndex, brush);
                board.Refresh();
            }));

    public Brush CursorBrush
    {
        set => SetValue(CursorBrushProperty, value);
        get => (Brush)GetValue(CursorBrushProperty);
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
            AdaptSpecialBrushArray();

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
            AdaptSpecialBrushArray();

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

    public event OnCellSelection? CellSelected;
    public event OnCellSelection? CellAddedToSelection;
    public event OnSelectionEnd? SelectionEnded;
    
    public NotifyingList<NeighborBorder> Borders { get; } = new();
    
    private readonly CellsAssociations _associatedCells;
    private bool _isSelecting;

    public TectonicBoard() : base(8)
    {
        Focusable = true;
        
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

        MouseLeftButtonDown += (_, args) =>
        {
            Focus();
            
            _isSelecting = true;
            var pos = ComputeSelectedCell(args.GetPosition(this));
            if (pos is not null) CellSelected?.Invoke(pos[0], pos[1]);
        };

        MouseMove += (_, args) =>
        {
            if (!_isSelecting) return;

            var pos = ComputeSelectedCell(args.GetPosition(this));
            if (pos is not null) CellAddedToSelection?.Invoke(pos[0], pos[1]);
        };

        MouseLeftButtonUp += StopSelection;
        MouseLeave += StopSelection;
    }
    
    public void AddChild(object value)
    {
        if (value is NeighborBorder border) Borders.Add(border);
    }

    public void AddText(string text)
    {
        
    }

    private void ReEvaluateNumberBrushes()
    {
        foreach (var component in Layers[NumbersIndex])
        {
            if (component is not SolutionComponent s) continue;
            
            var brush = _isSpecialNumberBrush[s.Row, s.Column] ? SpecialNumberBrush : DefaultNumberBrush;
            component.SetBrush(brush);
        }
    }

    private void AdaptSpecialBrushArray()
    {
        if (RowCount == 0 || ColumnCount == 0) _isSpecialNumberBrush = new bool[0, 0];
        else _isSpecialNumberBrush = new bool[RowCount, ColumnCount];
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
            Layers[NumbersIndex].Add(new SolutionComponent(number.ToString(), _cellSize * 3 / 4,
                _isSpecialNumberBrush[row, column] ? SpecialNumberBrush : DefaultNumberBrush, new Rect(GetLeft(column),
                    GetTop(row), _cellSize, _cellSize), row, column,
                        ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center));
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

    public void SetClue(int row, int column, bool isClue)
    {
        _isSpecialNumberBrush[row, column] = isClue;
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

    public void PutCursorOn(Cell cell)
    {
        ClearCursor();
        
        const double delta = CursorWidth / 2;
        var left = GetLeft(cell.Column);
        var top = GetTop(cell.Row);
        var pen = new Pen(CursorBrush, CursorWidth);

        var list = Layers[CursorIndex];
        list.Add(new LineComponent(new Point(left + delta, top), new Point(left + delta,
            top + _cellSize), pen));
        list.Add(new LineComponent(new Point(left, top + delta), new Point(left + _cellSize,
            top + delta), pen));
        list.Add(new LineComponent(new Point(left + _cellSize - delta, top), new Point(left + _cellSize - delta,
            top + _cellSize), pen));
        list.Add(new LineComponent(new Point(left, top + _cellSize - delta), new Point(left + _cellSize,
            top + _cellSize - delta), pen));
    }
    
    public void PutCursorOn(IZone cells)
    {
        ClearCursor();
        
        var delta = CursorWidth / 2;
        var pen = new Pen(CursorBrush, CursorWidth);

        var list = Layers[CursorIndex];
        foreach (var cell in cells)
        {
            var left = GetLeft(cell.Column);
            var top = GetTop(cell.Row);

            if(!cells.Contains(new Cell(cell.Row, cell.Column - 1))) list.Add(new LineComponent(
                new Point(left + delta, top), new Point(left + delta, top + _cellSize), pen));
            
            if(!cells.Contains(new Cell(cell.Row - 1, cell.Column))) list.Add(new LineComponent(
                new Point(left, top + delta), new Point(left + _cellSize, top + delta), pen));
            else
            {
                if(cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !cells.Contains(
                       new Cell(cell.Row - 1, cell.Column - 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left, top, CursorWidth, CursorWidth), CursorBrush));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row - 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top, CursorWidth, CursorWidth), CursorBrush));
            }
            
            if(!cells.Contains(new Cell(cell.Row, cell.Column + 1))) list.Add(new LineComponent(
                new Point(left + _cellSize - delta, top), new Point(left + _cellSize - delta, top + _cellSize), pen));
            
            if(!cells.Contains(new Cell(cell.Row + 1, cell.Column))) list.Add(new LineComponent(
                new Point(left, top + _cellSize - delta), new Point(left + _cellSize, top + _cellSize - delta), pen));
            else
            {
                if(cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column - 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), CursorBrush));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), CursorBrush));
            }
        }
    }
    
    public void PutCursorOn(IReadOnlyList<Cell> cells)
    {
        ClearCursor();
        
        var delta = CursorWidth / 2;
        var pen = new Pen(CursorBrush, CursorWidth);

        var list = Layers[CursorIndex];
        foreach (var cell in cells)
        {
            var left = GetLeft(cell.Column);
            var top = GetTop(cell.Row);

            if(!cells.Contains(new Cell(cell.Row, cell.Column - 1))) list.Add(new LineComponent(
                new Point(left + delta, top), new Point(left + delta, top + _cellSize), pen));
            
            if(!cells.Contains(new Cell(cell.Row - 1, cell.Column))) list.Add(new LineComponent(
                new Point(left, top + delta), new Point(left + _cellSize, top + delta), pen));
            else
            {
                if(cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !cells.Contains(
                       new Cell(cell.Row - 1, cell.Column - 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left, top, CursorWidth, CursorWidth), CursorBrush));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row - 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top, CursorWidth, CursorWidth), CursorBrush));
            }
            
            if(!cells.Contains(new Cell(cell.Row, cell.Column + 1))) list.Add(new LineComponent(
                new Point(left + _cellSize - delta, top), new Point(left + _cellSize - delta, top + _cellSize), pen));
            
            if(!cells.Contains(new Cell(cell.Row + 1, cell.Column))) list.Add(new LineComponent(
                new Point(left, top + _cellSize - delta), new Point(left + _cellSize, top + _cellSize - delta), pen));
            else
            {
                if(cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column - 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), CursorBrush));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), CursorBrush));
            }
        }
    }

    public void ClearCursor()
    {
        Layers[CursorIndex].Clear();
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
            new Rect(half, half, Width - _bigLineWidth, Height - _bigLineWidth), new Pen(LineBrush, _bigLineWidth)));

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
    
    private int[]? ComputeSelectedCell(Point point)
    {
        var row = 0;
        var col = 0;

        var y = point.Y;
        var x = point.X;

        for (; row < RowCount; row++)
        {
            if (y < _bigLineWidth) return null;
            y -= _bigLineWidth;
            if (y < _cellSize) break;
            y -= _cellSize;
        }

        for (; col < ColumnCount; col++)
        {
            if (x < _bigLineWidth) return null;
            x -= _bigLineWidth;
            if (x < _cellSize) break;
            x -= _cellSize;
        }

        return row == RowCount || col == ColumnCount ? null : new[] { row, col };
    }

    private void StopSelection(object sender, MouseEventArgs args)
    {
        if (!_isSelecting) return;

        _isSelecting = false;
        SelectionEnded?.Invoke();
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
public delegate void OnElementAdded<in T>(T element);
public delegate void OnDimensionCountChange(int number);