using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NewView;

public abstract class DrawingBoard : FrameworkElement
{
    private readonly DrawingVisual _visual = new();

    private readonly List<IDrawableComponent>[] _layers;
    
    protected IReadOnlyList<List<IDrawableComponent>> Layers => _layers;

    protected DrawingBoard(int layerCount)
    {
        Loaded += AddVisualToTree;
        Unloaded += RemoveVisualFromTree;

        _layers = new List<IDrawableComponent>[layerCount];
        for (int i = 0; i < _layers.Length; i++)
        {
            _layers[i] = new List<IDrawableComponent>();
        }
    }
    
    //DrawingNecessities------------------------------------------------------------------------------------------------
    
    // Provide a required override for the VisualChildrenCount property.
    protected override int VisualChildrenCount => 1;

    // Provide a required override for the GetVisualChild method.
    protected override Visual GetVisualChild(int index)
    {
        return _visual;
    }
    
    private void AddVisualToTree(object sender, RoutedEventArgs e)
    {
        AddVisualChild(_visual);
        AddLogicalChild(_visual);
    }

    private void RemoveVisualFromTree(object sender, RoutedEventArgs e)
    {
        RemoveLogicalChild(_visual);
        RemoveVisualChild(_visual);
    }
    
    //------------------------------------------------------------------------------------------------------------------
    
    public void Draw()
    {
        var context = _visual.RenderOpen();

        foreach (var list in _layers)
        {
            foreach (var component in list)
            {
                component.Draw(context);
            }
        }
        
        context.Close();
        InvalidateVisual();
    }

    public void Clear()
    {
        foreach (var list in _layers)
        {
            list.Clear();
        }
    }
    
    public BitmapFrame AsImage()
    {
        var rtb = new RenderTargetBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(_visual);
        return BitmapFrame.Create(rtb);
    }
}

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
    private readonly ComponentHorizontalAlignment _horizontalAlignment;
    private readonly ComponentVerticalAlignment _verticalAlignment;

    public TextInRectangleComponent(FormattedText text, Rect rect, ComponentHorizontalAlignment horizontalAlignment,
        ComponentVerticalAlignment verticalAlignment)
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

public enum ComponentVerticalAlignment
{
    Top = 0, Center = 1, Bottom = 2
}

public enum ComponentHorizontalAlignment
{
    Left = 0, Center = 1, Right = 2
}