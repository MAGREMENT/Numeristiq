using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter.Kakuros.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Tectonics.Controls;
using Model.Kakuros;

namespace DesktopApplication.View.Kakuros.Controls;

public class KakuroBoard : DrawingBoard, ISizeOptimizable, IKakuroSolverDrawer
{
    private const int BackgroundIndex = 0;
    private const int AmountLineIndex = 1;
    private const int AmountIndex = 2;
    private const int NumberLineIndex = 3;
    private const int NumberIndex = 4;

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
    
    public KakuroBoard() : base(4)
    {
    }

    public void SetSolution(int row, int col, int n)
    {
        if (!_numberPresence[row, col]) return;
        //TODO
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
        var w = _rowCount * _cellSize + _amountWidth + (_rowCount + 2) * _lineWidth;
        var h = _columnCount * _cellSize + _amountHeight + (_columnCount + 2) * _lineWidth;
        
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

        var half = _lineWidth / 2;
        foreach (var cell in _amountPresence)
        {
            if (cell.Orientation == Orientation.Vertical)
            {
                var xBr = GetLeft(cell.Column) + _cellSize + half;
                var yBr = cell.Row < 0 ? _amountHeight + _lineWidth + half : GetTop(cell.Row) + _cellSize + half;
                var xTr = xBr - _amountHeight;
                var yTr = yBr - _amountWidth;
                Layers[AmountLineIndex].Add(new LineComponent(new Point(xBr, yBr),
                    new Point(xTr, yTr), new Pen(AmountLineBrush, _lineWidth)));

                double xTl = GetLeft(cell.Column) - half;
                if (cell.Column <= 0 || cell.Row < 0 || !_numberPresence[cell.Row, cell.Column - 1])
                {
                    xTl -= _amountHeight;
                    if (!_amountPresence.Contains(new AmountCell(cell.Row, cell.Column - 1, Orientation.Vertical)))
                    {
                        var xBl = GetLeft(cell.Column) - half;
                        Layers[AmountLineIndex].Add(new LineComponent(new Point(xTl, yTr),
                            new Point(xBl, yBr), new Pen(AmountLineBrush, _lineWidth)));
                    }
                }
                
                Layers[AmountLineIndex].Add(new LineComponent(new Point(xTl, yTr),
                    new Point(xTr, yTr), new Pen(AmountLineBrush, _lineWidth)));
            }
            else
            {
                var xBr = GetLeft(cell.Column) + _cellSize + half;
                var yBr = GetTop(cell.Row) + _cellSize + half;
                var xBl = xBr - _amountHeight;
                var yBl = yBr - _amountHeight;
                Layers[AmountLineIndex].Add(new LineComponent(new Point(xBr, yBr),
                    new Point(xBl, yBl), new Pen(AmountLineBrush, _lineWidth)));

                var yTl = GetTop(cell.Row) - half;
                
                Layers[AmountLineIndex].Add(new LineComponent(new Point(xBl, yBl),
                    new Point(xBl, yTl), new Pen(AmountLineBrush, _lineWidth)));
            }
        }
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