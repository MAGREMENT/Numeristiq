using System;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.View.Controls;
using Model.Core.Changes;

namespace DesktopApplication.View.Themes.Controls;

public class ExampleBoard : DrawingBoard, ISizeOptimizable
{
    private const int RowCount = 5;
    private const int ColumnCount = 7;
    
    private double _cellSize;
    private double _lineWidth;

    public double CellSize
    {
        get => _cellSize;
        set
        {
            _cellSize = value;
            UpdateSize(true);
        }
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
    
    public Brush BackgroundBrush
    {
        set => SetValue(BackgroundBrushProperty, value);
        get => (Brush)GetValue(BackgroundBrushProperty);
    }
    
    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }
    
    protected override void Draw(DrawingContext context)
    {
        context.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, Width, Height));
        
        context.DrawRectangle(App.Current.ThemeInformation.ToBrush(StepColor.Change1), null,
            GetGridRect(0, 1));
        context.DrawRectangle(App.Current.ThemeInformation.ToBrush(StepColor.Change2), null,
            GetGridRect(0, 2));

        var color = (int)StepColor.Cause1;
        int row = 1;
        int col = 0;
        while (color <= (int)StepColor.Cause10)
        {
            context.DrawRectangle(App.Current.ThemeInformation.ToBrush((StepColor) color), null,
                GetGridRect(row, col));
            col++;
            if (col >= ColumnCount)
            {
                col = 0;
                row++;
            }

            color++;
        }

        var start = 0.0;
        for (int i = 0; i < ColumnCount + 1; i++)
        {
            context.DrawRectangle(LineBrush, null, new Rect(start, 0, _lineWidth, Height));
            start += _cellSize + _lineWidth;
        }

        start = 0;
        for (int i = 0; i < RowCount + 1; i++)
        {
            context.DrawRectangle(LineBrush, null, new Rect(0, start, Width, _lineWidth));
            start += _cellSize + _lineWidth;
        }
    }

    private Rect GetGridRect(int row, int col) => new(_lineWidth + col * (_lineWidth + _cellSize),
        _lineWidth + row * (_lineWidth + _cellSize), CellSize, CellSize);
    
    private void UpdateSize(bool fireEvent)
    {
        var w = _lineWidth + (_lineWidth + _cellSize) * ColumnCount;
        var h =  _lineWidth + (_lineWidth + _cellSize) * RowCount;
        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }

    public event OnSizeChange? OptimizableSizeChanged;
    public double GetWidthSizeMetricFor(double space)
    {
        return (space - _lineWidth * (ColumnCount + 1)) / ColumnCount;
    }

    public double GetHeightSizeMetricFor(double space)
    {
        return (space - _lineWidth * (RowCount + 1)) / RowCount;
    }

    public bool HasSize() => true;

    public void SetSizeMetric(double n)
    {
        _cellSize = n;
        UpdateSize(false);
    }
}