using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter.Nonograms.Solve;
using DesktopApplication.View.Controls;

namespace DesktopApplication.View.Nonograms.Controls;

public class NonogramBoard : DrawingBoard, ISizeOptimizable, INonogramDrawer
{
    private const int BackgroundIndex = 0;
    private const int LineIndex = 1;
    private const int NumberIndex = 2;
    private const int FillingIndex = 3;
    private const int UnavailableIndex = 4;

    private const int FillingShift = 3;
    private const int UnavailableThickness = 3;
    
    public static readonly DependencyProperty FillingBrushProperty =
        DependencyProperty.Register(nameof(FillingBrush), typeof(Brush), typeof(NonogramBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not NonogramBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(FillingIndex, brush);
                board.Refresh();
            }));
    
    public Brush FillingBrush
    {
        set => SetValue(FillingBrushProperty, value);
        get => (Brush)GetValue(FillingBrushProperty);
    }
    
    public static readonly DependencyProperty UnavailableBrushProperty =
        DependencyProperty.Register(nameof(UnavailableBrush), typeof(Brush), typeof(NonogramBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not NonogramBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(UnavailableIndex, brush);
                board.Refresh();
            }));
    
    public Brush UnavailableBrush
    {
        set => SetValue(UnavailableBrushProperty, value);
        get => (Brush)GetValue(UnavailableBrushProperty);
    }
    
    public static readonly DependencyProperty LineBrushProperty =
        DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(NonogramBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not NonogramBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(LineIndex, brush);
                board.Refresh();
            }));
    
    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }
    
    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(nameof(BackgroundBrush), typeof(Brush), typeof(NonogramBoard),
            new PropertyMetadata((obj, args) =>
            {
                if(obj is not NonogramBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(BackgroundIndex, brush);
                board.Refresh();
            }));

    public Brush BackgroundBrush
    {
        set => SetValue(BackgroundBrushProperty, value);
        get => (Brush)GetValue(BackgroundBrushProperty);
    }

    private readonly List<IReadOnlyList<int>> _rows = new();
    private readonly List<IReadOnlyList<int>> _columns = new();
    private int _maxDepth;
    private int _maxWidth;
    private double _cellSize;
    private double _lineWidth;

    public double CellSize
    {
        get => _cellSize;
        set
        {
            _cellSize = value;
            UpdateSize();
        }
    }

    public double LineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
            UpdateSize();
        }
    }
    
    public NonogramBoard() : base(5)
    {
    }

    public void SetRows(IEnumerable<IEnumerable<int>> rows)
    {
        _rows.Clear();
        _maxDepth = 0;
        foreach (var r in rows)
        {
            var asArray = r.ToArray();
            _maxDepth = Math.Max(asArray.Length, _maxDepth);
            _rows.Add(asArray);
        }
        
        OptimizableSizeChanged?.Invoke();
    }

    public void SetColumns(IEnumerable<IEnumerable<int>> cols)
    {
        _columns.Clear();
        _maxWidth = 0;
        foreach (var c in cols)
        {
            var asArray = c.ToArray();
            _maxWidth = Math.Max(asArray.Length, _maxWidth);
            _columns.Add(asArray);
        }
        
        OptimizableSizeChanged?.Invoke();
    }

    public void SetSolution(int row, int col)
    {
        var size = _cellSize - FillingShift * 2;
        Layers[FillingIndex].Add(new FilledRectangleComponent(new Rect(GetLeft(col) + FillingShift,
            GetTop(row) + FillingShift, size, size), FillingBrush));
    }

    public void ClearSolutions()
    {
        Layers[FillingIndex].Clear();
    }

    public void SetUnavailable(int row, int col)
    {
        var layer = Layers[UnavailableIndex];
        var shift = _cellSize / 4;
        var minX = GetLeft(col) + shift;
        var maxX = GetLeft(col) + _cellSize - shift;
        var minY = GetTop(row) + shift;
        var maxY = GetTop(row) + _cellSize - shift;
        
        layer.Add(new LineComponent(new Point(minX, minY), new Point(maxX, maxY),
            new Pen(UnavailableBrush, UnavailableThickness)));
        layer.Add(new LineComponent(new Point(minX, maxY), new Point(maxX, minY),
            new Pen(UnavailableBrush, UnavailableThickness)));
    }

    public void ClearUnavailable()
    {
        Layers[UnavailableIndex].Clear();
    }

    private double GetTop(int row)
    {
        return _maxDepth * _cellSize / 2 + _lineWidth + row * (_lineWidth + _cellSize);
    }

    private double GetLeft(int col)
    {
        return _maxWidth * _cellSize / 2 + _lineWidth + col * (_lineWidth + _cellSize);
    }

    private void UpdateSize()
    {
        Width = _lineWidth + (_lineWidth + _cellSize) * _columns.Count + _cellSize * _maxWidth / 2;
        Height = _lineWidth + (_lineWidth + _cellSize) * _rows.Count + _maxDepth * _cellSize / 2;
        
        Clear();
        SetBackground();
        SetNumbers();
        SetLines();
        Refresh();
    }

    private void SetNumbers()
    {
        var size = _cellSize / 3;
        var layer = Layers[NumberIndex];
        
        for (int col = 0; col < _columns.Count; col++)
        {
            var current = _columns[col];
            int depth = _maxDepth;
            for (int i = current.Count - 1; i >= 0; i--)
            {
                layer.Add(new TextInRectangleComponent(current[i].ToString(), size, LineBrush,
                    new Rect(GetLeft(col), _cellSize / 2 * (depth - 1), _cellSize, _cellSize / 2),
                    ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center));
                depth--;
            }
        }

        for (int row = 0; row < _rows.Count; row++)
        {
            var current = _rows[row];
            int width = _maxWidth;
            for (int i = current.Count - 1; i >= 0; i--)
            {
                layer.Add(new TextInRectangleComponent(current[i].ToString(), size, LineBrush,
                    new Rect(_cellSize / 2 * (width - 1), GetTop(row), _cellSize / 2, _cellSize),
                    ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center));
                width--;
            }
        }
    }

    private void SetLines()
    {
        if (!HasSize()) return;
        
        var yStart = _maxDepth * _cellSize / 2;
        var xStart = _maxWidth * _cellSize / 2;

        var currentY = yStart;
        for (int row = 0; row <= _rows.Count; row++)
        {
            Layers[LineIndex].Add(new FilledRectangleComponent(
                new Rect(xStart, currentY, Width - xStart, _lineWidth), LineBrush));

            currentY += _lineWidth + _cellSize;
        }
        
        var currentX = xStart;
        for (int col = 0; col <= _columns.Count; col++)
        {
            Layers[LineIndex].Add(new FilledRectangleComponent(
                new Rect(currentX, yStart, _lineWidth, Height - yStart), LineBrush));
            currentX += _lineWidth + _cellSize;
        }
    }
    
    private void SetBackground()
    {
        Layers[BackgroundIndex].Add(new FilledRectangleComponent(new Rect(0, 0, Width, Height), BackgroundBrush));
    }

    public event OnSizeChange? OptimizableSizeChanged;
    public int WidthSizeMetricCount => _columns.Count;
    public int HeightSizeMetricCount => _rows.Count;
    public double GetHeightAdditionalSize()
    {
        return _maxDepth * _cellSize / 2;
    }

    public double GetWidthAdditionalSize()
    {
        return _maxWidth * _cellSize / 2;
    }

    public bool HasSize()
    {
        return _rows.Count > 0 && _columns.Count > 0;
    }

    public double SimulateSizeMetric(int n, SizeType type)
    {
        return type == SizeType.Width
            ? _lineWidth + (_lineWidth + n) * _columns.Count + (double)(n * _maxWidth) / 2
            : _lineWidth + (_lineWidth + n) * _rows.Count + (double)(_maxDepth * n) / 2;
    }

    public void SetSizeMetric(int n)
    {
        CellSize = n;
    }
}