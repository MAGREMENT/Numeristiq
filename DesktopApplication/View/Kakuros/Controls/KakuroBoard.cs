using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter.Kakuros.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Tectonics.Controls;
using Model.Kakuros;
using Model.Utility;

namespace DesktopApplication.View.Kakuros.Controls;

public class KakuroBoard : DrawingBoard, ISizeOptimizable, IKakuroSolverDrawer
{
    private const int BackgroundIndex = 0;
    private const int CursorIndex = 1;
    private const int AmountLineIndex = 2;
    private const int AmountIndex = 3;
    private const int NumberLineIndex = 4;
    private const int NumberIndex = 5;

    private int _rowCount;
    private int _columnCount;
    private double _cellSize;
    private double _lineWidth;
    private double _amountWidth;
    private double _amountHeight;
    private double _amountWidthFactor;
    private double _amountHeightFactor;

    private bool[,] _numberPresence = new bool[0, 0];
    private readonly List<AmountCell> _amountPresence = new();

    public int RowCount
    {
        get => _rowCount;
        set
        {
            if (value == _rowCount) return;

            _rowCount = value;
            UpdatePresence();
            UpdateSize(true);
        }
    }
    
    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            if (value == _columnCount) return;

            _columnCount = value;
            UpdatePresence();
            UpdateSize(true);
        }
    }

    public double CellSize
    {
        get => _cellSize;
        set => SetCellSize(value, true);
    }

    private void SetCellSize(double value, bool fireEvent)
    {
        _cellSize = value;
        _amountWidth = _amountWidthFactor * _cellSize;
        _amountHeight = _amountHeightFactor * _cellSize;
        UpdateSize(fireEvent);
    }

    public double LineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
            UpdateSize(true);
        }
    }
    
    public double AmountWidthFactor
    {
        get => _amountWidthFactor;
        set
        {
            _amountWidthFactor = value;
            _amountWidth = value * _cellSize;
            UpdateSize(true);
        }
    }
    
    public double AmountHeightFactor
    {
        get => _amountHeightFactor;
        set
        {
            _amountHeightFactor = value;
            _amountHeight = value * _cellSize;
            UpdateSize(true);
        }
    }
    
    public static readonly DependencyProperty DefaultNumberBrushProperty =
        DependencyProperty.Register(nameof(DefaultNumberBrush), typeof(Brush), typeof(KakuroBoard),
            new PropertyMetadata((obj, _) =>
            {
                if(obj is not TectonicBoard board) return;
                board.Refresh();
            }));
    
    public Brush DefaultNumberBrush
    {
        set => SetValue(DefaultNumberBrushProperty, value);
        get => (Brush)GetValue(DefaultNumberBrushProperty);
    }
    
    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(nameof(BackgroundBrush), typeof(Brush), typeof(KakuroBoard),
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
    
    public static readonly DependencyProperty NumberLineBrushProperty =
        DependencyProperty.Register(nameof(NumberLineBrush), typeof(Brush), typeof(KakuroBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not TectonicBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(NumberLineIndex, brush);
                board.Refresh();
            }));
    
    public Brush NumberLineBrush
    {
        set => SetValue(NumberLineBrushProperty, value);
        get => (Brush)GetValue(NumberLineBrushProperty);
    }
    
    public static readonly DependencyProperty AmountLineBrushProperty =
        DependencyProperty.Register(nameof(AmountLineBrush), typeof(Brush), typeof(KakuroBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not TectonicBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(AmountLineIndex, brush);
                board.Refresh();
            }));
    
    public Brush AmountLineBrush
    {
        set => SetValue(AmountLineBrushProperty, value);
        get => (Brush)GetValue(AmountLineBrushProperty);
    }
    
    public static readonly DependencyProperty CursorBrushProperty =
        DependencyProperty.Register(nameof(CursorBrush), typeof(Brush), typeof(KakuroBoard),
            new PropertyMetadata((obj, args) =>
            {
                if (obj is not KakuroBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(CursorIndex, brush);
                board.Refresh();
            }));
    
    public Brush CursorBrush
    {
        set => SetValue(CursorBrushProperty, value);
        get => (Brush)GetValue(CursorBrushProperty);
    }

    public event OnCellSelection? CellSelected;
    
    public KakuroBoard() : base(6)
    {
        Focusable = true;
        
        MouseLeftButtonDown += (_, args) =>
        {
            Focus();
            var result = ComputeSelectedCell(args.GetPosition(this));
            if (result.Item1 is not null)
            {
                CellSelected?.Invoke(result.Item1.Value, result.Item2);
            }
        };
    }

    public void SetSolution(int row, int col, int n)
    {
        Dispatcher.Invoke(() =>
        {
            if (!_numberPresence[row, col]) return;

            Layers[NumberIndex].Add(new TextInRectangleComponent(n.ToString(), _cellSize * 3 / 4,
                DefaultNumberBrush, new Rect(GetLeft(col), GetTop(row), _cellSize, _cellSize),
                ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center));
        });
    }

    public void SetPossibilities(int row, int col, IEnumerable<int> poss)
    {
        Dispatcher.Invoke(() =>
        {
            var pSize = _cellSize / 3;
            foreach (var p in poss)
            {
                var l = GetLeft(col) + pSize * ((p - 1) % 3);
                // ReSharper disable once PossibleLossOfFraction
                var t = GetTop(row) + pSize * ((p - 1) / 3);
            
                Layers[NumberIndex].Add(new TextInRectangleComponent(p.ToString(), pSize * 3 / 4,
                    DefaultNumberBrush, new Rect(l, t, pSize, pSize),
                    ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center));
            }
        });
    }

    public void ClearCursor()
    {
        Layers[CursorIndex].Clear();
    }

    public void PutCursorOnNumberCell(int row, int col)
    {
        var half = _lineWidth / 2;
        Layers[CursorIndex].Clear();
        Layers[CursorIndex].Add(new OutlinedRectangleComponent(new Rect(GetLeft(col) + half,
            GetTop(row) + half, _cellSize - _lineWidth, _cellSize - _lineWidth), new Pen(CursorBrush, 3)));
    }

    public void PutCursorOnAmountCell(int row, int col, Orientation orientation)
    {
        Layers[CursorIndex].Clear();
        var half = _lineWidth / 3;
        if (orientation == Orientation.Vertical)
        {
            var xBr = GetLeft(col) + _cellSize - half;
            var yBr = row < 0 
                ? _amountHeight + _lineWidth + half - _lineWidth
                : GetTop(row) + _cellSize + half - _lineWidth;
            
            Layers[CursorIndex].Add(new LineComponent(new Point(xBr, yBr),
                new Point(GetLeftFull(col), yBr), new Pen(CursorBrush, _lineWidth)));
        }
        else
        {
            var xBr = GetLeft(col) + _cellSize + half - _lineWidth;
            var yBr = GetTop(row) + _cellSize - half;
            
            Layers[CursorIndex].Add(new LineComponent(new Point(xBr, yBr),
                new Point(xBr, GetTopFull(row)), new Pen(CursorBrush, _lineWidth)));
        }
    }

    public void ClearNumbers()
    {
        Dispatcher.Invoke(() => Layers[NumberIndex].Clear());
    }
    
    public void ClearAmounts()
    {
        Dispatcher.Invoke(() => Layers[AmountIndex].Clear());
    }

    public void SetAmount(int row, int col, int n, Orientation orientation)
    {
        if (orientation == Orientation.Vertical)
        {
            var t = row < 0 ? _lineWidth : GetTop(row) + _cellSize - _amountHeight;
            var l = GetLeft(col);
            Layers[AmountIndex].Add(new AmountComponent(n.ToString(), _amountHeight * 3 / 4,
                AmountLineBrush, new Rect(l, t, _amountWidth, _amountHeight), row, col, orientation)); 
        }
        else
        {
            var t = GetTop(row);
            var l = col < 0 ? _lineWidth : GetLeft(col) + _cellSize - _amountWidth;
            Layers[AmountIndex].Add(new AmountComponent(n.ToString(), _amountHeight * 3 / 4,
                AmountLineBrush, new Rect(l, t, _amountWidth, _amountHeight), row, col, orientation));
        }
    }
    
    public void ReplaceAmount(int row, int col, int n, Orientation orientation)
    {
        AmountComponent? comp = null;
        foreach (var component in Layers[AmountIndex])
        {
            if (component is not AmountComponent ac) continue;
            if (ac.IsSame(row, col, orientation))
            {
                comp = ac;
                break;
            }
        }

        if (comp is null) SetAmount(row, col, n, orientation);
        else comp.SetAmount(n);
    }

    public void ClearPresence()
    {
        Array.Clear(_numberPresence);
        _amountPresence.Clear();
    }

    public void SetPresence(int row, int col, bool value)
    {
        _numberPresence[row, col] = value;
    }

    public void SetPresence(int row, int col, Orientation orientation, bool value)
    {
        var c = new AmountCell(row, col, orientation);
        if (value) _amountPresence.Add(c);
        else _amountPresence.Remove(c);
    }
    
    public void RedrawLines()
    {
        Layers[NumberLineIndex].Clear();
        Layers[AmountLineIndex].Clear();
        SetLines();
    }
    
    #region Private

    private double GetLeft(int column) => 2 * _lineWidth + _amountWidth + column * (_lineWidth + _cellSize);

    private double GetTop(int row) => 2 * _lineWidth + _amountHeight + row * (_lineWidth + _cellSize);
    
    private double GetLeftFull(int column) => _lineWidth + _amountWidth + column * (_lineWidth + _cellSize);

    private double GetTopFull(int row) => _lineWidth + _amountHeight + row * (_lineWidth + _cellSize);

    private void UpdatePresence()
    {
        if (RowCount == _numberPresence.GetLength(0) && ColumnCount == _numberPresence.GetLength(1)) return;
        
        var buffer = new bool[RowCount, ColumnCount];
        for (int row = 0; row < RowCount && row < _numberPresence.GetLength(0); row++)
        {
            for (int col = 0; col < ColumnCount && col < _numberPresence.GetLength(1); col++)
            {
                buffer[row, col] = _numberPresence[row, col];
            }
        }
        _numberPresence = buffer;
    }

    private void UpdateSize(bool fireEvent)
    {
        var w = _columnCount * _cellSize + _amountWidth + (_columnCount + 2) * _lineWidth;
        var h = _rowCount * _cellSize + _amountHeight + (_rowCount + 2) * _lineWidth;
        
        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        
        Clear();
        SetBackground();
        SetLines();
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }

    private void SetBackground()
    {
        Layers[BackgroundIndex].Add(new FilledRectangleComponent(new Rect(0, 0, Width, Height), BackgroundBrush));
    }

    private void SetAmountCellLines(ICollection<IDrawableComponent> layer, AmountCell cell, Brush brush, double width)
    {
        var half = width / 2;
        if (cell.Orientation == Orientation.Vertical)
        {
            var xBr = GetLeft(cell.Column) + _cellSize + half;
            var yBr = cell.Row < 0 
                ? _amountHeight + _lineWidth + half
                : GetTop(cell.Row) + _cellSize + half;
            var xTr = xBr - _amountHeight;
            var yTr = yBr - _amountWidth;
            layer.Add(new LineComponent(new Point(xBr, yBr),
                new Point(xTr, yTr), new Pen(brush, _lineWidth)));

            double xTl = GetLeft(cell.Column) - half;
            if (cell.Column <= 0 || cell.Row < 0 || !_numberPresence[cell.Row, cell.Column - 1])
            {
                xTl -= _amountHeight;
                if (!_amountPresence.Contains(new AmountCell(cell.Row, cell.Column - 1, Orientation.Vertical)))
                {
                    var xBl = GetLeft(cell.Column) - half;
                    layer.Add(new LineComponent(new Point(xTl, yTr),
                        new Point(xBl, yBr), new Pen(brush, _lineWidth)));
                }

                xTl -= _lineWidth / 2;
            }
                
            layer.Add(new LineComponent(new Point(xTl, yTr),
                new Point(xTr, yTr), new Pen(brush, _lineWidth)));
        }
        else
        {
            var xBr = GetLeft(cell.Column) + _cellSize + half;
            var yBr = GetTop(cell.Row) + _cellSize + half;
            var xBl = xBr - _amountHeight;
            var yBl = yBr - _amountHeight;
            layer.Add(new LineComponent(new Point(xBr, yBr),
                new Point(xBl, yBl), new Pen(brush, _lineWidth)));

            var yTl = GetTop(cell.Row) - half;
            if (cell.Column < 0 || cell.Row <= 0 || !_numberPresence[cell.Row - 1, cell.Column])
            {
                yTl -= _amountWidth + _lineWidth / 2;
            }
                
            layer.Add(new LineComponent(new Point(xBl, yBl),
                new Point(xBl, yTl), new Pen(brush, _lineWidth)));
        }
    }

    private void SetLines()
    {
        if (_rowCount == 0 || _columnCount == 0) return;

        var l = _cellSize + _lineWidth * 2;

        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                if (!_numberPresence[row, col]) continue;
                
                Layers[NumberLineIndex].Add(new FilledRectangleComponent(new Rect(GetLeft(col) + _cellSize,
                    GetTopFull(row), _lineWidth, l), NumberLineBrush));
                Layers[NumberLineIndex].Add(new FilledRectangleComponent(new Rect(GetLeftFull(col),
                    GetTop(row) + _cellSize, l, _lineWidth), NumberLineBrush));
                
                if(row == 0 || !_numberPresence[row - 1, col])
                    Layers[NumberLineIndex].Add(new FilledRectangleComponent(new Rect(GetLeftFull(col),
                        GetTopFull(row), l, _lineWidth), NumberLineBrush));
                
                if(col == 0 || !_numberPresence[row, col - 1])
                    Layers[NumberLineIndex].Add(new FilledRectangleComponent(new Rect(GetLeftFull(col),
                        GetTopFull(row), _lineWidth, l), NumberLineBrush));
            }
        }

        foreach (var cell in _amountPresence)
        {
            SetAmountCellLines(Layers[AmountLineIndex], cell, AmountLineBrush, _lineWidth);
        }
    }
    
    private (Cell?, bool) ComputeSelectedCell(Point position)
    {
        var row = -2;
        var col = -2;

        var x = position.X;
        var y = position.Y;
        bool isAmountCell = false;

        if (x < _lineWidth) return (null, false);
        if (y < _lineWidth) return (null, false);

        x -= _lineWidth;
        y -= _lineWidth;

        if (x < _amountWidth)
        {
            col = -1;
            isAmountCell = true;
        }

        if (y < _amountHeight)
        {
            row = -1;
            isAmountCell = true;
        }

        x -= _amountWidth;
        y -= _amountHeight;

        if (col == -2)
        {
            for (int c = 0; c < ColumnCount; c++)
            {
                if(x < _lineWidth) return (null, false);
                x -= _lineWidth;
                if (x < _cellSize)
                {
                    col = c;
                    break;
                }

                x -= _cellSize;
            }
        }

        if (row == -2)
        {
            for (int r = 0; r < RowCount; r++)
            {
                if(y < _lineWidth) return (null, false);
                y -= _lineWidth;
                if (y < _cellSize)
                {
                    row = r;
                    break;
                }

                y -= _cellSize;
            }
        }

        if (row == -2 || col == -2) return (null, false);

        var cell = new Cell(row, col);
        if (isAmountCell)
        {
            if(IsAmountCellPresent(cell)) return (cell, true);
        }
        else
        {
            if (_numberPresence[row, col]) return (cell, false);
            if(IsAmountCellPresent(cell)) return (cell, true);
        }

        return (null, false);
    }

    private bool IsAmountCellPresent(Cell cell)
    {
        foreach (var c in _amountPresence)
        {
            if (c.Row == cell.Row && c.Column == cell.Column) return true;
        }

        return false;
    }
    
    #endregion

    public event OnSizeChange? OptimizableSizeChanged;
    public int WidthSizeMetricCount => ColumnCount;
    public int HeightSizeMetricCount => RowCount;

    public double GetHeightAdditionalSize() => _amountHeight + _lineWidth * (RowCount + 2);

    public double GetWidthAdditionalSize() => _amountWidth + _lineWidth * (ColumnCount + 2);

    public bool HasSize() => RowCount > 0 && ColumnCount > 0;

    public double SimulateSizeMetric(int n, SizeType type)
    {
        var buffer = n + _lineWidth;
        buffer *= type == SizeType.Width ? ColumnCount : RowCount;
        return buffer + _lineWidth * 2 + (type == SizeType.Width ? _amountWidthFactor : _amountHeightFactor) * n;
    }

    public void SetSizeMetric(int n)
    {
        SetCellSize(n, false);
    }
}

public readonly struct AmountCell
{
    public AmountCell(int row, int column, Orientation orientation)
    {
        Row = row;
        Column = column;
        Orientation = orientation;
    }

    public int Row { get; }
    public int Column { get; }
    public Orientation Orientation { get; }
}

public delegate void OnCellSelection(Cell cell, bool isAmountCell);

public class AmountComponent : TextInRectangleComponent
{
    private readonly int _row;
    private readonly int _col;
    private readonly Orientation _orientation;
    private readonly double _size;
    private readonly Brush _foreground;
    
    public AmountComponent(string text, double size, Brush foreground, Rect rect, int row, int col,
        Orientation orientation) : base(text, size, foreground, rect, ComponentHorizontalAlignment.Center,
        ComponentVerticalAlignment.Center)
    {
        _size = size;
        _foreground = foreground;
        _row = row;
        _col = col;
        _orientation = orientation;
    }

    public bool IsSame(int row, int col, Orientation orientation) => col == _col && row == _row &&
                                                     orientation == _orientation;

    public void SetAmount(int n) => _text = new FormattedText(n.ToString(), Info,
        Flow, Typeface, _size, _foreground, 1);
}