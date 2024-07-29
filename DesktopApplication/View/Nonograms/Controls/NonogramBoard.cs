using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter.Nonograms.Solve;
using DesktopApplication.View.Controls;
using Model.Core.Changes;
using Model.Utility;

namespace DesktopApplication.View.Nonograms.Controls;

public class NonogramBoard : DrawingBoard, INonogramDrawingData, ISizeOptimizable, INonogramDrawer
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
    
    public static readonly DependencyProperty LineBrushProperty =
        DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(NonogramBoard),
            new PropertyMetadata((obj, _) =>
            {
                if(obj is not NonogramBoard board) return;
                board.Refresh();
            }));

    public Brush CursorBrush => Brushes.Transparent;
    public Brush LinkBrush => Brushes.Transparent;

    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }
    
    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(nameof(BackgroundBrush), typeof(Brush), typeof(NonogramBoard),
            new PropertyMetadata((obj, _) =>
            {
                if(obj is not NonogramBoard board) return;
                board.Refresh();
            }));

    public Brush BackgroundBrush
    {
        set => SetValue(BackgroundBrushProperty, value);
        get => (Brush)GetValue(BackgroundBrushProperty);
    }

    public double InwardCellLineWidth => 3;

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

    public double GetLeftOfCellWithBorder(int col)
    {
        return GetLeftOfCell(col) - _bigLineWidth;
    }

    public double GetTopOfCell(int row)
    {
        return MaxDepth * _cellSize / 2 + _bigLineWidth + row * (_bigLineWidth + _cellSize);
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

    public bool IsClue(int row, int col) => false;

    public double GetLeftOfCell(int col)
    {
        return MaxWideness * _cellSize / 2 + _bigLineWidth + col * (_bigLineWidth + _cellSize);
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
        MaxDepth = 0;
        foreach (var r in rows)
        {
            var asArray = r.ToArray();
            MaxDepth = Math.Max(asArray.Length, MaxDepth);
            _rows.Add(asArray);
        }
        
        UpdateSize(true);
    }

    public void SetColumns(IEnumerable<IEnumerable<int>> cols)
    {
        _columns.Clear();
        MaxWideness = 0;
        foreach (var c in cols)
        {
            var asArray = c.ToArray();
            MaxWideness = Math.Max(asArray.Length, MaxWideness);
            _columns.Add(asArray);
        }
        
        UpdateSize(true);
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

    public void EncircleCells(HashSet<Cell> cells, StepColor color)
    {
        //TODO
    }

    public void HighlightValues(int unit, int startIndex, int endIndex, StepColor color, Orientation orientation)
    {
        Layers[HighlightIndex].Add(new ValuesHighlightDrawableComponent(unit, startIndex, endIndex, color, orientation));
    }

    public void EncircleSection(int unit, int startIndex, int endIndex, StepColor color, Orientation orientation)
    {
        Layers[HighlightIndex].Add(new CellSectionDrawableComponent(unit, startIndex, endIndex, color, orientation));
    }

    #region Private

    private void UpdateSize(bool fireEvent)
    {
        var w = _bigLineWidth + (_bigLineWidth + _cellSize) * _columns.Count + _cellSize * MaxWideness / 2;
        var h = _bigLineWidth + (_bigLineWidth + _cellSize) * _rows.Count + MaxDepth * _cellSize / 2;
        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }

    #endregion

    #region ISizeOptimizable

    public event OnSizeChange? OptimizableSizeChanged;
    public int WidthSizeMetricCount => _columns.Count;
    public int HeightSizeMetricCount => _rows.Count;
    
    public double GetHeightAdditionalSize()
    {
        return MaxDepth * _cellSize / 2 + _bigLineWidth * (_rows.Count + 1);
    }

    public double GetWidthAdditionalSize()
    {
        return MaxWideness * _cellSize / 2 + _bigLineWidth * (_columns.Count + 1);
    }

    public bool HasSize()
    {
        return _rows.Count > 0 && _columns.Count > 0;
    }

    public double SimulateSizeMetric(int n, SizeType type)
    {
        return type == SizeType.Width
            ? _bigLineWidth + (_bigLineWidth + n) * _columns.Count + (double)(n * MaxWideness) / 2
            : _bigLineWidth + (_bigLineWidth + n) * _rows.Count + (double)(MaxDepth * n) / 2;
    }

    public void SetSizeMetric(int n)
    {
        _cellSize = n;
        UpdateSize(false);
    }

    #endregion
}