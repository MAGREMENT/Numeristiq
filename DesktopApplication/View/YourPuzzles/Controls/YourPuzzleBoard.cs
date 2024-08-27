using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Tectonics.Solve;
using DesktopApplication.Presenter.YourPuzzles;
using DesktopApplication.View.Controls;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.View.YourPuzzles.Controls;

public class YourPuzzleBoard : DrawingBoard, IVaryingBordersCellGameDrawingData, ISizeOptimizable, IYourPuzzleDrawer
{
    private const int BackgroundIndex = 0;
    private const int CursorIndex = 1;
    private const int GridIndex = 2;
    private const int GreaterThanIndex = 3;
    private const int NumbersIndex = 4;

    private double _bigLineWidth;
    private double _smallLineWidth;
    private int _rowCount;
    private int _columnCount;
    private double _cellSize;

    private readonly Dictionary<NeighborBorder, bool> _borders = new();

    public event OnCellSelection? CellSelected;
    public event OnCellSelection? CellAddedToSelection;

    public event OnDimensionCountChange? RowCountChanged;
    public event OnDimensionCountChange? ColumnCountChanged;
    
    public YourPuzzleBoard() : base(5)
    {
        Layers[BackgroundIndex].Add(new BackgroundDrawableComponent());
        Layers[GridIndex].Add(new VaryingBordersGridDrawableComponent());
        
        MouseLeftButtonDown += (_, args) =>
        {
            Focus();
            
            var pos = ComputeSelectedCell(args.GetPosition(this));
            if (pos is not null) CellSelected?.Invoke(pos[0], pos[1]);
        };

        MouseMove += (_, args) =>
        {
            if(args.LeftButton != MouseButtonState.Pressed) return;

            var pos = ComputeSelectedCell(args.GetPosition(this));
            if (pos is not null) CellAddedToSelection?.Invoke(pos[0], pos[1]);
        };
    }

    #region DrawingData

    public Brush BackgroundBrush
    {
        get => (Brush)GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    } 
    public Typeface Typeface { get; } = new(new FontFamily(new Uri("pack://application:,,,/View/Fonts/"), "./#Roboto Mono"),
        FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);
    public CultureInfo CultureInfo { get; } =  CultureInfo.CurrentUICulture;

    public Brush CursorBrush
    {
        get => (Brush)GetValue(CursorBrushProperty);
        set => SetValue(CursorBrushProperty, value);
    } 
    public Brush LinkBrush => (Brush)GetValue(LinkBrushProperty);

    public Brush LineBrush
    {
        get => (Brush)GetValue(LineBrushProperty); 
        set => SetValue(LineBrushProperty, value);
    }
    public double BigLineWidth
    {
        get => _bigLineWidth;
        set
        {
            _bigLineWidth = value;
            UpdateSize(true);
        }
    }
    public double SmallLineWidth
    {
        get => _smallLineWidth;
        set
        {
            _smallLineWidth = value;
            UpdateSize(true);
        }
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
    public int RowCount
    {
        get => _rowCount;
        set
        {
            _rowCount = value;
            UpdateSize(true);
            RowCountChanged?.Invoke(_rowCount);
        }
    }
    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            _columnCount = value;
            UpdateSize(true);
            ColumnCountChanged?.Invoke(_columnCount);
        }
    }
    public double LinkOffset => 20;
    public LinkOffsetSidePriority LinkOffsetSidePriority { get; set; } = LinkOffsetSidePriority.Any; 
    public double GetLeftOfCell(int col)
    {
        return _bigLineWidth + col * (_bigLineWidth + _cellSize);
    }

    public double GetLeftOfCellWithBorder(int col)
    {
        return col * (_bigLineWidth + _cellSize);
    }

    public double GetTopOfCell(int row)
    {
        return _bigLineWidth + row * (_bigLineWidth + _cellSize);
    }

    public double GetTopOfCellWithBorder(int row)
    {
        return row * (_bigLineWidth + _cellSize);
    }

    public Point GetCenterOfCell(int row, int col)
    {
        var half = _cellSize / 2;
        return new Point(GetLeftOfCell(col) + half, GetTopOfCell(row) + half);
    }

    public bool IsThin(BorderDirection direction, int row, int col)
    {
        return _borders.TryGetValue(new NeighborBorder(row, col, direction), out var b) && b;
    }

    #endregion

    #region Drawer

    public void ClearHighlights()
    {
        
    }
    
    public void PutCursorOn(IContainingEnumerable<Cell> cells)
    {
        ClearCursor();

        Layers[CursorIndex].Add(new InwardMultiCellDrawableComponent(cells, InwardBrushType.Cursor));
    }

    public void ClearCursor()
    {
        Layers[CursorIndex].Clear();
    }
    
    public void ClearBorderDefinitions()
    {
        _borders.Clear();
    }
    
    public void AddBorderDefinition(int insideRow, int insideColumn, BorderDirection direction, bool isThin)
    {
        _borders[new NeighborBorder(insideRow, insideColumn, direction)] = isThin;
    }

    public void ClearGreaterThanSigns()
    {
        Layers[GreaterThanIndex].Clear();
    }

    public void AddGreaterThanSign(Cell smaller, Cell greater)
    {
        Layers[GreaterThanIndex].Add(new GreaterThanDrawableComponent(smaller, greater));
    }

    #endregion

    #region Private

    private void UpdateSize(bool fireEvent)
    {
        var w = _cellSize * _columnCount + _bigLineWidth * (_columnCount + 1);
        var h = _cellSize * _rowCount + _bigLineWidth * (_rowCount + 1);
        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
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

    #endregion

    #region ISizeOptimizable

    public event OnSizeChange? OptimizableSizeChanged;
    public double GetWidthSizeMetricFor(double space)
    {
        return (space - _bigLineWidth * (_columnCount + 1)) / _columnCount;
    }

    public double GetHeightSizeMetricFor(double space)
    {
        return (space - _bigLineWidth * (_rowCount + 1)) / _rowCount;
    }

    public bool HasSize()
    {
        return RowCount > 0 && ColumnCount > 0;
    }

    public void SetSizeMetric(double n)
    {
        _cellSize = n;
        UpdateSize(false);
    }

    #endregion
}