using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace View;

public interface IDrawableComponent
{
    void Draw(DrawingContext context);
}

public class FilledRectangleComponent : IDrawableComponent
{
    private readonly Rect _rect;
    private readonly Brush _brush;

    public FilledRectangleComponent(Rect rect, Brush brush)
    {
        _rect = rect;
        _brush = brush;
    }

    public void Draw(DrawingContext context)
    {
        context.DrawRectangle(_brush, null, _rect);
    }
}

public class OutlinedRectangleComponent : IDrawableComponent
{
    private readonly Rect _rect;
    private readonly Pen _pen;

    public OutlinedRectangleComponent(Rect rect, Pen pen)
    {
        _rect = rect;
        _pen = pen;
    }

    public void Draw(DrawingContext context)
    {
        context.DrawRectangle(null, _pen, _rect);
    }
}

public class FilledPolygonComponent : IDrawableComponent
{
    private readonly Point[] _points;
    private readonly Brush _brush;

    public FilledPolygonComponent(Brush brush, params Point[] points)
    {
        _brush = brush;
        _points = points;
    }

    public FilledPolygonComponent(Brush brush, IReadOnlyList<Point> points)
    {
        _points = points.ToArray();
        _brush = brush;
    }

    public void Draw(DrawingContext context)
    {
        var segmentCollection = new PathSegmentCollection();
        for (int i = 1; i < _points.Length; i++)
        {
            segmentCollection.Add(new LineSegment(_points[i], true));
        }
        
        var geometry = new PathGeometry
        {
            Figures = new PathFigureCollection
            {
                new()
                {
                    IsClosed = true,
                    StartPoint = _points[0],
                    Segments = segmentCollection
                }
            }
        };

        context.DrawGeometry(_brush, null, geometry);
    }
}

public class LineComponent : IDrawableComponent
{
    private readonly Point _from;
    private readonly Point _to;
    private readonly Pen _pen;

    public LineComponent(Point from, Point to, Pen pen)
    {
        _from = from;
        _to = to;
        _pen = pen;
    }

    public void Draw(DrawingContext context)
    {
        context.DrawLine(_pen, _from, _to);
    }
}

public class TextInRectangleComponent : IDrawableComponent
{
    private readonly FormattedText _text;
    private readonly Rect _rect;
    private readonly TextHorizontalAlignment _horizontalAlignment;
    private readonly TextVerticalAlignment _verticalAlignment;

    public TextInRectangleComponent(FormattedText text, Rect rect, TextHorizontalAlignment horizontalAlignment,
        TextVerticalAlignment verticalAlignment)
    {
        _text = text;
        _rect = rect;
        _horizontalAlignment = horizontalAlignment;
        _verticalAlignment = verticalAlignment;
    }

    public void Draw(DrawingContext context)
    {
        var deltaX = (_rect.Width - _text.Width) / 2;
        var deltaY = (_rect.Height - _text.Height) / 2;
            
        context.DrawText(_text, new Point(_rect.X + deltaX * (int)_horizontalAlignment, 
            _rect.Y + deltaY * (int)_verticalAlignment));
    }
}

public delegate void OnCellSelection(int row, int col);

public enum TextVerticalAlignment
{
    Top = 0, Center = 1, Bottom = 2
}

public enum TextHorizontalAlignment
{
    Left = 0, Center = 1, Right = 2
}