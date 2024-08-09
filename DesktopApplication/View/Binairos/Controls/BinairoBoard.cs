using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DesktopApplication.Presenter.Binairos.Solve;
using DesktopApplication.View.Controls;

namespace DesktopApplication.View.Binairos.Controls;

public class BinairoBoard : DrawingBoard, IBinairoDrawingData, ISizeOptimizable, IBinairoDrawer
{
    private const int BackgroundIndex = 0;
    private const int LinesIndex = 1;
    private const int NumbersIndex = 2;

    private double _lineWidth;
    private double _cellSize;
    private int _rowCount;
    private int _columnCount;

    private bool[,] _clues = new bool[0, 0];

    public BinairoBoard() : base(3)
    {
        Layers[BackgroundIndex].Add(new BackgroundDrawableComponent());
        Layers[LinesIndex].Add(new BinairoGridDrawableComponent());
    }

    #region DrawingData

    public Typeface Typeface { get; } = new(new FontFamily(new Uri("pack://application:,,,/View/Fonts/"), "./#Roboto Mono"),
        FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);
    public CultureInfo CultureInfo { get; } =  CultureInfo.CurrentUICulture;
    
    public Brush CircleFirstColor 
    {
        set => SetValue(CircleFirstColorProperty, value);
        get => (Brush)GetValue(CircleFirstColorProperty);
    }
    public Brush CircleSecondColor
    {
        set => SetValue(CircleSecondColorProperty, value);
        get => (Brush)GetValue(CircleSecondColorProperty);
    }
    
    public static readonly DependencyProperty CircleFirstColorProperty =
        DependencyProperty.Register(nameof(CircleFirstColor), typeof(Brush), typeof(DrawingBoard),
            new PropertyMetadata((obj, _) =>
    {
        if (obj is not DrawingBoard board) return;
        board.Refresh();
    }));
    
    public static readonly DependencyProperty CircleSecondColorProperty =
        DependencyProperty.Register(nameof(CircleSecondColor), typeof(Brush), typeof(DrawingBoard),
            new PropertyMetadata((obj, _) =>
    {
        if (obj is not DrawingBoard board) return;
        board.Refresh();
    }));

    public Brush DefaultNumberBrush
    {
        set => SetValue(DefaultNumberBrushProperty, value);
        get => (Brush)GetValue(DefaultNumberBrushProperty);
    }

    public Brush ClueNumberBrush
    {
        set => SetValue(ClueNumberBrushProperty, value);
        get => (Brush)GetValue(ClueNumberBrushProperty);
    }

    public Brush BackgroundBrush
    {
        set => SetValue(BackgroundBrushProperty, value);
        get => (Brush)GetValue(BackgroundBrushProperty);
    }

    public Brush LinkBrush
    {
        set => SetValue(LinkBrushProperty, value);
        get => (Brush)GetValue(LinkBrushProperty);
    }

    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }
    
    public Brush CursorBrush
    {
        set => SetValue(CursorBrushProperty, value);
        get => (Brush)GetValue(CursorBrushProperty);
    }
    
    public int RowCount
    {
        get => _rowCount;
        set
        {
            _rowCount = value;
            AdaptClueArray();
            UpdateSize(true);
        }
    }

    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            _columnCount = value;
            AdaptClueArray();
            UpdateSize(true);
        }
    }

    public double BigLineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
            UpdateSize(true);
        }
    }

    public double SmallLineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
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

    public double LinkOffset => 20;
    public LinkOffsetSidePriority LinkOffsetSidePriority { get; set; } = LinkOffsetSidePriority.Any;
    public bool AreSolutionNumbers { get; set; } = true;
    public double GetLeftOfCell(int col)
    {
        return _lineWidth + (_lineWidth + _cellSize) * col;
    }

    public double GetLeftOfCellWithBorder(int col)
    {
        return (_lineWidth + _cellSize) * col;
    }

    public double GetTopOfCell(int row)
    {
        return _lineWidth + (_lineWidth + _cellSize) * row;
    }

    public double GetTopOfCellWithBorder(int row)
    {
        return (_lineWidth + _cellSize) * row;
    }

    public Point GetCenterOfCell(int row, int col)
    {
        var delta = _cellSize / 2;
        return new Point(GetLeftOfCell(col) + delta, GetTopOfCell(row) + delta);
    }

    public bool IsClue(int row, int col) => _clues[row, col];

    #endregion

    public void ClearSolutions()
    {
        Dispatcher.Invoke(() => Layers[NumbersIndex].Clear());
    }

    public void ShowSolution(int solution, int row, int col)
    {
        Dispatcher.Invoke(() => Layers[NumbersIndex].Add(new BinairoSolutionDrawableComponent(solution, row, col)));
    }

    public void SetClue(int row, int col, bool isClue)
    {
        _clues[row, col] = isClue;
    }
    
    public void ClearHighlights()
    {
        
    }

    private void AdaptClueArray()
    {
        var buffer = new bool[RowCount, ColumnCount];
        for (int row = 0; row < _clues.GetLength(0) && row < RowCount; row++)
        {
            for (int col = 0; col < _clues.GetLength(1) && col < ColumnCount; col++)
            {
                buffer[row, col] = _clues[row, col];
            }
        }

        _clues = buffer;
    }
    
    private void UpdateSize(bool fireEvent)
    {
        var w = _columnCount * (_lineWidth + _cellSize) + _lineWidth;
        var h = _rowCount * (_lineWidth + _cellSize) + _lineWidth;
        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }

    #region ISizeOptimizable

    public event OnSizeChange? OptimizableSizeChanged;
    public int WidthSizeMetricCount => _columnCount;
    public int HeightSizeMetricCount => _rowCount;
    public double GetHeightAdditionalSize()
    {
        return _lineWidth * (_rowCount + 1);
    }

    public double GetWidthAdditionalSize()
    {
        return _lineWidth * (_columnCount + 1);
    }

    public bool HasSize()
    {
        return _rowCount > 0 && _columnCount > 0;
    }

    public double SimulateSizeMetric(int n, SizeType type)
    {
        var mult = type == SizeType.Height ? _rowCount : _columnCount;
        return _lineWidth + (_lineWidth + n) * mult;
    }

    public void SetSizeMetric(int n)
    {
        _cellSize = n;
        UpdateSize(false);
    }

    #endregion
}