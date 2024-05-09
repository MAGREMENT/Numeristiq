using System;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.View.Tectonics.Controls;
using DesktopApplication.View.Utility;

namespace DesktopApplication.View.Kakuros.Controls;

public class KakuroBoard : DrawingBoard
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

    private bool[,] _numberPresence = new bool[0, 0];
    private bool[] _rowAmountPresence = Array.Empty<bool>();
    private bool[] _columnAmountPresence = Array.Empty<bool>();

    public int RowCount
    {
        get => _rowCount;
        set
        {
            if (value == _rowCount) return;

            _rowCount = value;
            UpdatePresence();
            UpdateSize();
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
            UpdateSize();
        }
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

    public double LineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
            UpdateSize();
        }
    }
    
    public double AmountWidth
    {
        get => _amountWidth;
        set
        {
            _amountWidth = value;
            UpdateSize();
        }
    }
    
    public double AmountHeight
    {
        get => _amountHeight;
        set
        {
            _amountHeight = value;
            UpdateSize();
        }
    }
    
    public static readonly DependencyProperty DefaultNumberBrushProperty =
        DependencyProperty.Register(nameof(DefaultNumberBrush), typeof(Brush), typeof(TectonicBoard),
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
                board.SetLayerBrush(NumberLineIndex, brush);
                board.Refresh();
            }));
    
    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }
    
    public KakuroBoard() : base(4)
    {
    }

    public void SetSolution(int row, int col, int n)
    {
        if (!_numberPresence[row, col]) return;
        //TODO
    }
    
    #region Private

    private double GetLeft(int column) => 2 * _lineWidth + _amountWidth + column * (_lineWidth + _cellSize);

    private double GetTop(int row) => 2 * _lineWidth + _amountHeight + row * (_lineWidth + _cellSize);
    
    private double GetLeftFull(int column) => _lineWidth + _amountWidth + column * (_lineWidth + _cellSize);

    private double GetTopFull(int row) => _lineWidth + _amountHeight + row * (_lineWidth + _cellSize);

    private void UpdatePresence()
    {
        if (RowCount != _numberPresence.GetLength(0) || ColumnCount != _numberPresence.GetLength(1))
        {
            var buffer = new bool[RowCount, ColumnCount];
            Array.Copy(_numberPresence, buffer, _numberPresence.Length);
            _numberPresence = buffer;
        }

        if (RowCount != _rowAmountPresence.Length) _rowAmountPresence = _rowAmountPresence.CopyToNewSize(RowCount);
        if (ColumnCount != _columnAmountPresence.Length) _columnAmountPresence = _columnAmountPresence.CopyToNewSize(ColumnCount);
    }

    private void UpdateSize()
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
                    GetTopFull(row), _lineWidth, l), LineBrush));
                Layers[NumberLineIndex].Add(new FilledRectangleComponent(new Rect(GetLeftFull(col),
                    GetTop(row) + _cellSize, l, _lineWidth), LineBrush));
                
                if(row == 0 || _numberPresence[row - 1, col])
                    Layers[NumberLineIndex].Add(new FilledRectangleComponent(new Rect(GetLeftFull(col),
                        GetTopFull(row), l, _lineWidth), LineBrush));
                
                if(col == 0 || _numberPresence[row, col - 1])
                    Layers[NumberLineIndex].Add(new FilledRectangleComponent(new Rect(GetLeftFull(col),
                        GetTopFull(row), _lineWidth, l), LineBrush));
            }
        }
    }
    
    #endregion
}