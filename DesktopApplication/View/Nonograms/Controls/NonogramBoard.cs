﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Nonograms.Solve;
using DesktopApplication.View.Controls;
using Model.Core.Changes;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.View.Nonograms.Controls;

public class NonogramBoard : LayeredDrawingBoard, INonogramDrawingData, ISizeOptimizable, INonogramDrawer
{
    private const int BackgroundIndex = 0;
    private const int LineIndex = 1;
    private const int HighlightIndex = 2;
    private const int FillingIndex = 3;
    private const int UnavailableIndex = 4;
    private const int NumbersIndex = 5;

    private readonly List<IReadOnlyList<int>> _rows = new();
    private readonly List<IReadOnlyList<int>> _columns = new();
    private double _cellSize;
    private double _bigLineWidth;

    public event OnDimensionCountChange? RowCountChanged;
    public event OnDimensionCountChange? ColumnCountChanged;
    
    public NonogramBoard() : base(6)
    {
        Layers[BackgroundIndex].Add(new BackgroundDrawableComponent());
        Layers[LineIndex].Add(new NonogramGridDrawableComponent());
        Layers[NumbersIndex].Add(new NonogramNumbersDrawableComponent());
    }

    #region INonogramDrawingData
    
    public Typeface Typeface { get; } = new(new FontFamily(new Uri("pack://application:,,,/View/Fonts/"), "./#Roboto Mono"),
        FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);
    public CultureInfo CultureInfo { get; } =  CultureInfo.CurrentUICulture;
    
    public static readonly DependencyProperty FillingBrushProperty =
        DependencyProperty.Register(nameof(FillingBrush), typeof(Brush), typeof(NonogramBoard),
            new PropertyMetadata((obj, _) =>
            {
                if(obj is not NonogramBoard board) return;
                board.Refresh();
            }));
    
    public Brush FillingBrush
    {
        set => SetValue(FillingBrushProperty, value);
        get => (Brush)GetValue(FillingBrushProperty);
    }
    
    public static readonly DependencyProperty UnavailableBrushProperty =
        DependencyProperty.Register(nameof(UnavailableBrush), typeof(Brush), typeof(NonogramBoard),
            new PropertyMetadata((obj, _) =>
            {
                if(obj is not NonogramBoard board) return;
                board.Refresh();
            }));
    
    public Brush UnavailableBrush
    {
        set => SetValue(UnavailableBrushProperty, value);
        get => (Brush)GetValue(UnavailableBrushProperty);
    }

    public Brush CursorBrush => Brushes.Transparent;
    public Brush LinkBrush => Brushes.Transparent;

    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }

    public Brush BackgroundBrush
    {
        set => SetValue(BackgroundBrushProperty, value);
        get => (Brush)GetValue(BackgroundBrushProperty);
    }

    public double InwardCellLineWidth => 5;

    public double CellSize
    {
        get => _cellSize;
        set
        {
            _cellSize = value;
            UpdateSize(true);
        }
    }

    public double LinkOffset => 20;
    public LinkOffsetSidePriority LinkOffsetSidePriority { get; set; } = LinkOffsetSidePriority.Any;

    public double BigLineWidth
    {
        get => _bigLineWidth;
        set
        {
            _bigLineWidth = value;
            UpdateSize(true);
        }
    }

    public double SmallLineWidth => BigLineWidth;
    public double FillingShift => 3;
    public double UnavailableThickness => 3;

    public double GetLeftOfCell(int col)
    {
        return MaxWideness * _cellSize / 2.0 + _bigLineWidth + col * (_bigLineWidth + _cellSize);
    }
    
    public double GetLeftOfCellWithBorder(int col)
    {
        return GetLeftOfCell(col) - _bigLineWidth;
    }

    public double GetTopOfCell(int row)
    {
        return MaxDepth * _cellSize / 2.0 + _bigLineWidth + row * (_bigLineWidth + _cellSize);
    }

    public double GetTopOfCellWithBorder(int row)
    {
        return GetTopOfCell(row) - _bigLineWidth;
    }

    public Point GetCenterOfCell(int row, int col)
    {
        var delta = _cellSize / 2;
        return new Point(GetLeftOfCell(col) + delta, GetTopOfCell(row) + delta);
    }

    public int RowCount => _rows.Count;
    public int ColumnCount => _columns.Count;
    public int MaxDepth { get; private set; }

    public int MaxWideness { get; private set; }

    public IReadOnlyList<int> GetRowValues(int row)
    {
        return _rows[row];
    }

    public IReadOnlyList<int> GetColumnValues(int col)
    {
        return _columns[col];
    }

    #endregion

    public void SetRows(IEnumerable<IEnumerable<int>> rows)
    {
        _rows.Clear();
        MaxWideness = 1;
        foreach (var r in rows)
        {
            var asArray = r.ToArray();
            MaxWideness = Math.Max(asArray.Length, MaxWideness);
            _rows.Add(asArray);
        }
        
        UpdateSize(true);
        RowCountChanged?.Invoke(_rows.Count);
    }

    public void SetColumns(IEnumerable<IEnumerable<int>> cols)
    {
        _columns.Clear();
        MaxDepth = 1;
        foreach (var c in cols)
        {
            var asArray = c.ToArray();
            MaxDepth = Math.Max(asArray.Length, MaxDepth);
            _columns.Add(asArray);
        }
        
        UpdateSize(true);
        ColumnCountChanged?.Invoke(_columns.Count);
    }

    public void SetSolution(int row, int col)
    {
        Dispatcher.Invoke(() =>
        {
            Layers[FillingIndex].Add(new SolutionDrawableComponent(row, col));
        });
    }

    public void ClearSolutions()
    {
        Dispatcher.Invoke(() => Layers[FillingIndex].Clear());
    }

    public void SetUnavailable(int row, int col)
    {
        Dispatcher.Invoke(() =>
        {
            Layers[UnavailableIndex].Add(new UnavailabilityDrawableComponent(row, col));
        });
    }

    public void ClearUnavailable()
    {
        Dispatcher.Invoke(() => Layers[UnavailableIndex].Clear());
    }

    public void ClearHighlights()
    {
        Layers[HighlightIndex].Clear();
    }

    public void HighlightValues(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color)
    {
        Layers[HighlightIndex].Add(new ValuesHighlightDrawableComponent(unit, startIndex, endIndex, color, orientation));
    }

    public void EncircleCells(IContainingEnumerable<Cell> cells, StepColor color)
    {
        Layers[HighlightIndex].Add(new MultiCellGeometryDrawableComponent(cells, (int)color, FillColorType.Step));
    }

    public void EncircleLineSection(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color)
    {
        Layers[HighlightIndex].Add(new CellSectionDrawableComponent(unit, startIndex, endIndex, color, orientation));
    }

    #region Private

    private void UpdateSize(bool fireEvent)
    {
        var w = _bigLineWidth + (_bigLineWidth + _cellSize) * _columns.Count + _cellSize * MaxWideness / 2.0;
        var h = _bigLineWidth + (_bigLineWidth + _cellSize) * _rows.Count + _cellSize * MaxDepth / 2.0;
        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }

    #endregion

    #region ISizeOptimizable

    public event OnSizeChange? OptimizableSizeChanged;
    
    public double GetWidthSizeMetricFor(double space)
    {
        return (space - _bigLineWidth * (_columns.Count + 1)) / (_columns.Count + MaxWideness / 2.0);
    }

    public double GetHeightSizeMetricFor(double space)
    {
        return (space - _bigLineWidth * (_rows.Count + 1)) / (_rows.Count + MaxDepth / 2.0);
    }

    public bool HasSize()
    {
        return _rows.Count > 0 && _columns.Count > 0;
    }

    public void SetSizeMetric(double n)
    {
        _cellSize = n;
        UpdateSize(false);
    }

    #endregion
}