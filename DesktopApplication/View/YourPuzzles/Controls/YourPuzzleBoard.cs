using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Tectonics.Solve;
using DesktopApplication.Presenter.YourPuzzles;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Tectonics.Controls;

namespace DesktopApplication.View.YourPuzzles.Controls;

public class YourPuzzleBoard : DrawingBoard, IVaryingBordersCellGameData, ISizeOptimizable, IYourPuzzleDrawer
{
    private const int BackgroundIndex = 0;
    private const int GridIndex = 1;
    private const int NumbersIndex = 2;

    private double _bigLineWidth;
    private double _smallLineWidth;
    private int _rowCount;
    private int _columnCount;
    private double _cellSize;
    
    public YourPuzzleBoard() : base(3)
    {
        Layers[BackgroundIndex].Add(new BackgroundDrawableComponent());
        Layers[GridIndex].Add(new VaryingBordersGridDrawableComponent());
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
    public Brush CursorBrush => (Brush)GetValue(CursorBrushProperty);
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
        }
    }
    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            _columnCount = value;
            UpdateSize(true);
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
        return _smallLineWidth + col * (_bigLineWidth + _cellSize);
    }

    public double GetTopOfCell(int row)
    {
        return _bigLineWidth + row * (_bigLineWidth + _cellSize);
    }

    public double GetTopOfCellWithBorder(int row)
    {
        return _smallLineWidth + row * (_bigLineWidth + _cellSize);
    }

    public Point GetCenterOfCell(int row, int col)
    {
        var half = _cellSize / 2;
        return new Point(GetLeftOfCell(col) + half, GetTopOfCell(row) + half);
    }

    public NeighborBorder? GetBorder(BorderDirection direction, int row, int col)
    {
        return null;
    }

    #endregion

    #region Drawer

    public void ClearHighlights()
    {
        throw new NotImplementedException();
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