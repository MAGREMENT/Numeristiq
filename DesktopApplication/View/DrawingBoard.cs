using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DesktopApplication.View;

public abstract class DrawingBoard : FrameworkElement
{
    private readonly DrawingVisual _visual = new();
    private readonly List<IDrawableComponent>[] _layers;

    protected IReadOnlyList<List<IDrawableComponent>> Layers => _layers;
    public bool RefreshAllowed { get; set; } = true;
    public bool AntiAliasing { get; set; } = true;

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

    #region DrawingNecessities

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

    #endregion
    
    public void Refresh()
    {
        Dispatcher.Invoke(() =>
        {
            if (!RefreshAllowed) return;
            
            var context = _visual.RenderOpen();

            foreach (var list in _layers)
            {
                foreach (var component in list)
                {
                    component.Draw(context, !AntiAliasing);
                }
            }
            
            context.Close();
            InvalidateVisual();
        });
    }

    public void Clear()
    {
        foreach (var list in _layers)
        {
            list.Clear();
        }
    }
    
    public void SetLayerBrush(int index, Brush brush)
    {
        foreach (var comp in _layers[index])
        {
            comp.SetBrush(brush);
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
    void Draw(DrawingContext context, bool withGuidelines);
    void SetBrush(Brush brush);
}

public class FilledRectangleComponent : IDrawableComponent
{
    private readonly Rect _rect;
    private Brush _brush;

    public FilledRectangleComponent(Rect rect, Brush brush)
    {
        _rect = rect;
        _brush = brush;
    }

    public void Draw(DrawingContext context, bool withGuidelines)
    {
        if (withGuidelines)
        {
            var gSet = new GuidelineSet();
            gSet.GuidelinesX.Add(_rect.Left);
            gSet.GuidelinesX.Add(_rect.Right);
            gSet.GuidelinesY.Add(_rect.Top);
            gSet.GuidelinesY.Add(_rect.Bottom);
            context.PushGuidelineSet(gSet);
        }
        
        context.DrawRectangle(_brush, null, _rect);
        
        if(withGuidelines) context.Pop();
    }

    public void SetBrush(Brush brush)
    {
        _brush = brush;
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

    public virtual void Draw(DrawingContext context, bool withGuidelines)
    {
        if (withGuidelines)
        {
            var gSet = new GuidelineSet();
            var half = _pen.Thickness / 2;
            gSet.GuidelinesX.Add(_rect.Left - half);
            gSet.GuidelinesX.Add(_rect.Right + half);
            gSet.GuidelinesY.Add(_rect.Top - half);
            gSet.GuidelinesY.Add(_rect.Bottom + half);
            context.PushGuidelineSet(gSet);
        }
        
        context.DrawRectangle(null, _pen, _rect);
        
        if(withGuidelines) context.Pop();
    }

    public void SetBrush(Brush brush)
    {
        _pen.Brush = brush;
    }
}

public class AngledOutlinedRectangleComponent : OutlinedRectangleComponent
{
    private readonly double _angle;
    
    public AngledOutlinedRectangleComponent(Rect rect, Pen pen, double angle) : base(rect, pen)
    {
        _angle = angle;
    }

    public override void Draw(DrawingContext context, bool withGuidelines)
    {
        context.PushTransform(new RotateTransform(_angle));
        base.Draw(context, withGuidelines);
        context.Pop();
    }
}

public class FilledPolygonComponent : IDrawableComponent
{
    private readonly Point[] _points;
    private Brush _brush;

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

    public void Draw(DrawingContext context, bool withGuidelines)
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

    public void SetBrush(Brush brush)
    {
        _brush = brush;
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

    public void Draw(DrawingContext context, bool withGuidelines)
    {
        if (withGuidelines)
        {
            var gSet = new GuidelineSet();
            gSet.GuidelinesX.Add(_from.X);
            gSet.GuidelinesX.Add(_to.X);
            gSet.GuidelinesY.Add(_from.Y);
            gSet.GuidelinesY.Add(_to.Y);
            context.PushGuidelineSet(gSet);
        }
        
        context.DrawLine(_pen, _from, _to);

        if (withGuidelines) context.Pop();
    }

    public void SetBrush(Brush brush)
    {
        _pen.Brush = brush;
    }
}

public class TextInRectangleComponent : IDrawableComponent
{
    protected static readonly CultureInfo Info = CultureInfo.CurrentUICulture;
    protected const FlowDirection Flow = FlowDirection.LeftToRight;
    protected static readonly Typeface Typeface = new("Lucida Sans Typewriter");
    
    protected FormattedText _text;
    private readonly Rect _rect;
    private readonly ComponentHorizontalAlignment _horizontalAlignment;
    private readonly ComponentVerticalAlignment _verticalAlignment;

    public TextInRectangleComponent(string text, double size, Brush foreground, Rect rect,
        ComponentHorizontalAlignment horizontalAlignment, ComponentVerticalAlignment verticalAlignment)
    {
        _text = new FormattedText(text, Info, Flow, Typeface, size, foreground, 1);
        _rect = rect;
        _horizontalAlignment = horizontalAlignment;
        _verticalAlignment = verticalAlignment;
    }

    public virtual void Draw(DrawingContext context, bool withGuidelines)
    {
        var deltaX = (_rect.Width - _text.Width) / 2;
        var deltaY = (_rect.Height - _text.Height) / 2;
            
        context.DrawText(_text, new Point(_rect.X + deltaX * (int)_horizontalAlignment, 
            _rect.Y + deltaY * (int)_verticalAlignment));
    }

    public void SetBrush(Brush brush)
    {
        _text.SetForegroundBrush(brush);
    }
}

public class AngledTextInRectangleComponent : TextInRectangleComponent
{
    private readonly double _angle;
    
    public AngledTextInRectangleComponent(string text, double size, Brush foreground, Rect rect,
        ComponentHorizontalAlignment horizontalAlignment, ComponentVerticalAlignment verticalAlignment, double angle)
        : base(text, size, foreground, rect, horizontalAlignment, verticalAlignment)
    {
        _angle = angle;
    }

    public override void Draw(DrawingContext context, bool withGuidelines)
    {
        context.PushTransform(new RotateTransform(_angle));
        base.Draw(context, withGuidelines);
        context.Pop();
    }
}

public class SolutionComponent : TextInRectangleComponent
{
    public int Row { get; }
    public int Column { get; }
    
    public SolutionComponent(string text, double size, Brush foreground, Rect rect, int row, int col
        , ComponentHorizontalAlignment horizontalAlignment, ComponentVerticalAlignment verticalAlignment)
        : base(text, size, foreground, rect, horizontalAlignment, verticalAlignment)
    {
        Row = row;
        Column = col;
    }
}

public delegate void OnCellSelection(int row, int col);

public delegate void OnSelectionEnd();

public enum ComponentVerticalAlignment
{
    Top = 0, Center = 1, Bottom = 2
}

public enum ComponentHorizontalAlignment
{
    Left = 0, Center = 1, Right = 2
}