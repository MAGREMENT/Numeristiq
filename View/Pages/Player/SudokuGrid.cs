using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Global;
using Global.Enums;
using View.Utility;
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

    //This is a hack for the MouseLeftDown event to work properly. Do NOT remove.
    private readonly RectAndBrush _backGround;
    
    private readonly List<RectAndBrush> _numbersHighlight = new();
    private RectAndPen? _cursor;
    private readonly List<RectAndBrush> _smallMargins = new();
    private readonly List<RectAndBrush> _bigMargins = new();
    private readonly List<TextAndRect> _numbers = new();
    private readonly List<RectAndPen> _encircles = new();
    private readonly List<LineAndPen> _highlightLines = new();

    public event OnCellSelection? CellSelected;

    public SudokuGrid(int possibilitySize, int smallLineWidth, int bigLineWidth)
    {
        Focusable = true;
        
        _visual = new VisualCollection(this);
        
        _possibilitySize = possibilitySize;
        _smallLineWidth = smallLineWidth;
        _bigLineWidth = bigLineWidth;
        _cellSize = _possibilitySize * 3;
        
        _size = ComputeSize();
        Width = _size;
        Height = _size;
        _backGround = new RectAndBrush(new Rect(0, 0, _size, _size), Brushes.White);
        
        UpdateMargins();
        Refresh();

        MouseLeftButtonDown += (_, args) =>
        {
            Focus();
            var cell = ComputeSelectedCell(args.GetPosition(this));
            if (cell is not null) CellSelected?.Invoke(cell[0], cell[1]);
        };
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
        _highlightLines.Clear();
    }

    public void ClearCursor()
    {
        _cursor = null;
    }
    
    public void SetPossibility(int row, int col, int possibility, Brush color)
    {
        var text = new FormattedText(possibility.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Verdaba"),  (double)_possibilitySize / 4 * 3, color, 1);
        _numbers.Add(new TextAndRect(text, new Rect(GetLeft(col, possibility), GetTop(row, possibility), _possibilitySize, _possibilitySize)));
    }

    public void SetSolution(int row, int col, int possibility, Brush color)
    {
        var text = new FormattedText(possibility.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Verdaba"), (double)_cellSize / 4 * 3, color, 1);
        _numbers.Add(new TextAndRect(text, new Rect(GetLeft(col), GetTop(row), _cellSize, _cellSize)));
    }
    
    public void Refresh()
    {
        var visual = new DrawingVisual();
        var context = visual.RenderOpen();

        context.DrawRectangle(_backGround.Brush, null, _backGround.Rect);

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

        foreach (var line in _highlightLines)
        {
            context.DrawLine(line.Pen, line.From, line.To);
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
            _possibilitySize + _smallLineWidth, _possibilitySize + _smallLineWidth), new Pen(StrongLinkBrush, _bigLineWidth)));
    }

    public void PutCursorOn(int row, int col)
    {
        var delta = CursorWidth / 2;
        _cursor = new RectAndPen(new Rect(GetLeft(col) + delta, GetTop(row) + delta,
            _cellSize - CursorWidth, _cellSize - CursorWidth), new Pen(ColorManager.Purple, CursorWidth));
    }
    
    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo,
        int possibilityTo, Color color)
    {
        var delta = (double)_smallLineWidth / 2;
        
        var xFrom = GetLeft(colFrom, possibilityFrom) - delta;
        var yFrom = GetTop(rowFrom, possibilityFrom) - delta;
        
        var xTo = GetLeft(colTo, possibilityTo) - delta;
        var yTo = GetTop(rowTo, possibilityTo) - delta;

        double leftX, topY, rightX, bottomY;

        if (xFrom < xTo)
        {
            leftX = xFrom;
            rightX = xTo + _possibilitySize + _smallLineWidth;
        }
        else
        {
            leftX = xTo;
            rightX =xFrom + _possibilitySize + _smallLineWidth;
        }

        if (yFrom < yTo)
        {
            topY = yFrom;
            bottomY = yTo + _possibilitySize + _smallLineWidth;
        }
        else
        {
            topY = yTo;
            bottomY = yFrom + _possibilitySize + _smallLineWidth;
        }
        
        _encircles.Add(new RectAndPen(new Rect(new Point(leftX, topY), new Point(rightX, bottomY)),
            new Pen(new SolidColorBrush(color), _bigLineWidth)));
    }
    
    public void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, Color color)
    {
        var delta = (double)_bigLineWidth / 2;
        
        var xFrom = GetLeft(colFrom) - delta;
        var yFrom = GetTop(rowFrom) - delta;
        
        var xTo = GetLeft(colTo) - delta;
        var yTo = GetTop(rowTo) - delta;

        double leftX, topY, rightX, bottomY;

        if (xFrom < xTo)
        {
            leftX = xFrom;
            rightX = xTo + _cellSize + _bigLineWidth;
        }
        else
        {
            leftX = xTo;
            rightX = xFrom + _cellSize +  _bigLineWidth;
        }

        if (yFrom < yTo)
        {
            topY = yFrom;
            bottomY = yTo + _cellSize + _bigLineWidth;
        }
        else
        {
            topY = yTo;
            bottomY = yFrom + _cellSize +  _bigLineWidth;
        }
        
        _encircles.Add(new RectAndPen(new Rect(new Point(leftX, topY), new Point(rightX, bottomY)),
            new Pen(new SolidColorBrush(color), _bigLineWidth)));
    }

    public void EncircleCellPatch(Cell[] cells, Color color)
    {
        var delta = (double)_bigLineWidth / 2;
        
        foreach (var cell in cells)
        {
            var topLeftX = GetLeft(cell.Column) - delta;
            var topLeftY = GetTop(cell.Row) - delta;

            var bottomRightX = topLeftX + _cellSize + _bigLineWidth;
            var bottomRightY = topLeftY + _cellSize + _bigLineWidth;

            if (!cells.Contains(new Cell(cell.Row, cell.Column + 1)))
            {
                _highlightLines.Add(new LineAndPen(new Point(bottomRightX, topLeftY),
                    new Point(bottomRightX, bottomRightY), new Pen(new SolidColorBrush(color), _bigLineWidth)));
            }

            if (!cells.Contains(new Cell(cell.Row, cell.Column - 1)))
            {
                _highlightLines.Add(new LineAndPen(new Point(topLeftX, topLeftY),
                    new Point(topLeftX, bottomRightY), new Pen(new SolidColorBrush(color), _bigLineWidth)));
            }
            
            if (!cells.Contains(new Cell(cell.Row + 1, cell.Column)))
            {
                _highlightLines.Add(new LineAndPen(new Point(topLeftX, bottomRightY),
                    new Point(bottomRightX, bottomRightY), new Pen(new SolidColorBrush(color), _bigLineWidth)));
            }

            if (!cells.Contains(new Cell(cell.Row - 1, cell.Column)))
            {
                _highlightLines.Add(new LineAndPen(new Point(topLeftX, topLeftY),
                    new Point(bottomRightX, topLeftY), new Pen(new SolidColorBrush(color), _bigLineWidth)));
            }
        }
    }

    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo, bool isWeak,
        LinkOffsetSidePriority priority)
    {
        var from = Center(rowFrom, colFrom, possibilityFrom);
        var to = Center(rowTo, colTo, possibilityTo);
        var middle = new Point(from.X + (to.X - from.X) / 2, from.Y + (to.Y - from.Y) / 2);

        var offsets = MathUtility.ShiftSecondPointPerpendicularly(from, middle, LinkOffset);

        var validOffsets = new List<Point>();
        for (int i = 0; i < 2; i++)
        {
            var p = offsets[i];
            if(p.X > 0 && p.X < _size && p.Y > 0 && p.Y < _size) validOffsets.Add(p);
        }

        switch (validOffsets.Count)
        {
            case 0 : 
                AddShortenedLine(from, to, isWeak);
                break;
            case 1 :
                AddShortenedLine(from, validOffsets[0], to, isWeak);
                break;
            case 2 :
                if(priority == LinkOffsetSidePriority.Any) AddShortenedLine(from, validOffsets[0], to, isWeak);
                else
                {
                    var left = MathUtility.IsLeft(from, to, validOffsets[0]) ? 0 : 1;
                    if(priority == LinkOffsetSidePriority.Left) AddShortenedLine(from, validOffsets[left], to, isWeak);
                    else AddShortenedLine(from, validOffsets[(left + 1) % 2], to, isWeak);
                }
                break;
        }
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
    
    private void AddShortenedLine(Point from, Point to, bool isWeak)
    {
        var shortening = _possibilitySize / 2;

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var mag = Math.Sqrt(dx * dx + dy * dy);
        var newFrom = new Point(from.X + shortening * dx / mag, from.Y + shortening * dy / mag);
        var newTo = new Point(to.X - shortening * dx / mag, to.Y - shortening * dy / mag);
        
        AddLine(newFrom, newTo, isWeak);
    }
    
    private void AddShortenedLine(Point from, Point middle, Point to, bool isWeak)
    {
        var shortening = _possibilitySize / 2;
        
        var dxFrom = middle.X - from.X;
        var dyFrom = middle.Y - from.Y;
        var mag = Math.Sqrt(dxFrom * dxFrom + dyFrom * dyFrom);
        var newFrom = new Point(from.X + shortening * dxFrom / mag, from.Y + shortening * dyFrom / mag);

        var dxTo = to.X - middle.X;
        var dyTo = to.Y - middle.Y;
        mag = Math.Sqrt(dxTo * dxTo + dyTo * dyTo);
        var newTo = new Point(to.X - shortening * dxTo / mag, to.Y - shortening * dyTo / mag);
            
        AddLine(newFrom, middle, isWeak);
        AddLine(middle, newTo, isWeak);
    }

    private void AddLine(Point from, Point to, bool isWeak)
    {
        _highlightLines.Add(new LineAndPen(from, to, new Pen(StrongLinkBrush, 2)
        {
            DashStyle = isWeak ? DashStyles.DashDot : DashStyles.Solid
        }));
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

    private Point Center(int row, int col, int possibility)
    {
        var delta = (double)_possibilitySize / 2;
        return new Point(GetLeft(col, possibility) + delta, GetTop(row, possibility) + delta);
    }

    private int[]? ComputeSelectedCell(Point point)
    {
        var row = -1;
        var col = -1;

        var y = point.Y;
        var x = point.X;

        for (int i = 0; i < 9; i++)
        {
            var delta = i % 3 == 0 ? _bigLineWidth : _smallLineWidth;

            if (row == -1)
            {
                if (y < delta) return null;
                y -= delta;
                if (y < _cellSize) row = i;
                y -= _cellSize;
            }

            if (col == -1)
            {
                if (x < delta) return null;
                x -= delta;
                if (x < _cellSize) col = i;
                x -= _cellSize;
            }

            if (row != -1 && col != -1) break;
        }

        return row == -1 || col == -1 ? null : new[] { row, col };
    }
}   

public record RectAndBrush(Rect Rect, Brush Brush);
public record RectAndPen(Rect Rect, Pen Pen);
public record LineAndPen(Point From, Point To, Pen Pen);
public record TextAndRect(FormattedText Text, Rect Rect);
public delegate void OnCellSelection(int row, int col);
