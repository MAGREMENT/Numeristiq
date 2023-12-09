using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace View.Pages.Player;

public class SudokuGrid : FrameworkElement
{
    private static readonly Brush StrongLinkBrush = Brushes.Indigo;
    private const double LinkOffset = 20;
    private const double CursorWidth = 2;
    
    private readonly int _possibilitySize;
    private readonly int _cellSize;
    private readonly int _smallLineWidth;
    private readonly int _bigLineWidth;
    private readonly double _size;
    
    private readonly VisualCollection _visual;
    
    private readonly List<RectAndBrush> _numbersHighlight = new();
    private RectAndPen? _cursor;
    private readonly List<RectAndBrush> _smallMargins = new();
    private readonly List<RectAndBrush> _bigMargins = new();
    private readonly List<TextAndRect> _numbers = new();
    private readonly List<RectAndPen> _encircles = new();

    public SudokuGrid(int possibilitySize, int smallLineWidth, int bigLineWidth)
    {
        _visual = new VisualCollection(this);
        
        _possibilitySize = possibilitySize;
        _smallLineWidth = smallLineWidth;
        _bigLineWidth = bigLineWidth;
        _cellSize = _possibilitySize * 3;
        
        _size = ComputeSize();
        Width = _size;
        Height = _size;
        
        UpdateMargins();
        InitNumbers();
        Refresh();
    }
    
    // Provide a required override for the VisualChildrenCount property.
    protected override int VisualChildrenCount => _visual.Count;

    // Provide a required override for the GetVisualChild method.
    protected override Visual GetVisualChild(int index)
    {
        if (index < 0 || index >= _visual.Count)
        {
            throw new ArgumentOutOfRangeException();
        }

        return _visual[index];
    }
    
    public void ClearNumbers()
    {
        _numbers.Clear();
    }

    public void ClearHighlighting()
    {
        _numbersHighlight.Clear();
        _encircles.Clear();
    }

    public void ClearCursor()
    {
        _cursor = null;
    }
    
    public void SetPossibility(int row, int col, int possibility)
    {
        var text = new FormattedText(possibility.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Verdaba"),  (double)_possibilitySize / 4 * 3, Brushes.Black, 1);
        _numbers.Add(new TextAndRect(text, new Rect(GetLeft(col, possibility), GetTop(row, possibility), _possibilitySize, _possibilitySize)));
    }

    public void SetSolution(int row, int col, int possibility)
    {
        var text = new FormattedText(possibility.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Verdaba"), (double)_cellSize / 4 * 3, Brushes.Black, 1);
        _numbers.Add(new TextAndRect(text, new Rect(GetLeft(col), GetTop(row), _cellSize, _cellSize)));
    }
    
    public void Refresh()
    {
        var visual = new DrawingVisual();
        var context = visual.RenderOpen();

        foreach (var rect in _numbersHighlight)
        {
            context.DrawRectangle(rect.Brush, null, rect.Rect);
        }

        if (_cursor is not null) context.DrawRectangle(null, _cursor.Pen, _cursor.Rect);
        
        foreach (var rect in _smallMargins)
        {
            context.DrawRectangle(rect.Brush, null, rect.Rect);
        }
        
        foreach (var rect in _bigMargins)
        {
            context.DrawRectangle(rect.Brush, null, rect.Rect);
        }

        foreach (var text in _numbers)
        {
            var deltaX = (text.Rect.Width - text.Text.Width) / 2;
            var deltaY = (text.Rect.Height - text.Text.Height) / 2;
            
            context.DrawText(text.Text, new Point(text.Rect.X + deltaX, text.Rect.Y + deltaY));
        }

        foreach (var rect in _encircles)
        {
            context.DrawRectangle(null, rect.Pen, rect.Rect);
        }
        
        context.Close();
        _visual.Clear();
        _visual.Add(visual);
    }
    
    public void FillCell(int row, int col, Color color)
    {
        _numbersHighlight.Add(new RectAndBrush(new Rect(GetLeft(col), GetTop(row), _cellSize, _cellSize),
            new SolidColorBrush(color)));
    }

    public void FillPossibility(int row, int col, int possibility, Color color)
    {
        _numbersHighlight.Add(new RectAndBrush(new Rect(GetLeft(col, possibility), GetTop(row, possibility),
                _possibilitySize, _possibilitySize), new SolidColorBrush(color)));
    }

    public void EncircleCell(int row, int col)
    {
        var delta = (double)_bigLineWidth / 2;
        _encircles.Add(new RectAndPen(new Rect(GetLeft(col) - delta, GetTop(row) - delta,
            _cellSize + _bigLineWidth, _cellSize + _bigLineWidth), new Pen(StrongLinkBrush, _bigLineWidth)));
    }
    
    public void EncirclePossibility(int row, int col, int possibility)
    {
        var delta = (double)_smallLineWidth / 2;
        _encircles.Add(new RectAndPen(new Rect(GetLeft(col, possibility) - delta, GetTop(row, possibility) - delta,
            _cellSize + _smallLineWidth, _cellSize + _smallLineWidth), new Pen(StrongLinkBrush, _smallLineWidth)));
    }

    public void PutCursorOn(int row, int col)
    {
        var delta = CursorWidth / 2;
        _cursor = new RectAndPen(new Rect(GetLeft(col) + delta, GetTop(row) + delta,
            _cellSize - CursorWidth, _cellSize - CursorWidth), new Pen(Brushes.Black, CursorWidth));
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private double ComputeSize()
    {
        return _cellSize * 9 + _smallLineWidth * 6 + _bigLineWidth * 4;
    }

    private void UpdateMargins()
    {
        var delta = _bigLineWidth + _cellSize;
        for (int i = 0; i < 6; i++)
        {
            _smallMargins.Add(new RectAndBrush(new Rect(0, delta, _size, _smallLineWidth), Brushes.Black));
            _smallMargins.Add(new RectAndBrush(new Rect(delta, 0, _smallLineWidth, _size), Brushes.Black));

            delta += i % 2 == 0 ? _smallLineWidth + _cellSize : _smallLineWidth + _cellSize + _bigLineWidth + _cellSize;
        }

        delta = 0;
        for (int i = 0; i < 4; i++)
        {
            _bigMargins.Add(new RectAndBrush(new Rect(0, delta, _size, _bigLineWidth), Brushes.Black));
            _bigMargins.Add(new RectAndBrush(new Rect(delta, 0, _bigLineWidth, _size), Brushes.Black));

            delta += _cellSize * 3 + _smallLineWidth * 2 + _bigLineWidth;
        }
    }

    private void InitNumbers()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    SetPossibility(row, col, number);
                }
            }
        }
    }

    private double GetTop(int row)
    {
        var miniRow = row / 3;
        return row * _cellSize + miniRow * _bigLineWidth + _bigLineWidth + (row - miniRow) * _smallLineWidth;
    }

    private double GetTop(int row, int possibility)
    {
        var miniRow = row / 3;
        var posRow = (possibility - 1) / 3;
        return row * _cellSize + posRow * _possibilitySize + miniRow * _bigLineWidth + _bigLineWidth
               + (row - miniRow) * _smallLineWidth;
    }
    
    private double GetLeft(int col)
    {
        var miniCol = col / 3;
        return col * _cellSize + miniCol * _bigLineWidth + _bigLineWidth + (col - miniCol) * _smallLineWidth;
    }
    
    private double GetLeft(int col, int possibility)
    {
        var miniCol = col / 3;
        var posCol = (possibility - 1) % 3;
        return col * _cellSize + posCol * _possibilitySize + miniCol * _bigLineWidth + _bigLineWidth
               + (col - miniCol) * _smallLineWidth;
    }
}

public record RectAndBrush(Rect Rect, Brush Brush);
public record RectAndPen(Rect Rect, Pen Pen);
public record TextAndRect(FormattedText Text, Rect Rect);