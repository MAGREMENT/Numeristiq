using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopApplication.View.Utility;
using Model.Core.Changes;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using MathUtility = DesktopApplication.View.Utility.MathUtility;
using Point = System.Windows.Point;

namespace DesktopApplication.View;

/// <summary>
/// TODO Change the way components work. Right now, Their information is fixed and they need to be cleared when
/// updating size. Make them fetch their information from the drawing board when drawn.
/// </summary>
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

public interface IDC
{
    void Draw(DrawingContext context, object data);
}

public interface IDC<in T> : IDC
{
    void Draw(DrawingContext context, T data);

    void IDC.Draw(DrawingContext context, object data)
    {
        if (data is T t) Draw(context, t);
    }
}

public static class DCHelper
{
    public static void DrawTextInRectangle(DrawingContext context, FormattedText _text, Rect rect,
        ComponentHorizontalAlignment ha, ComponentVerticalAlignment va)
    {
        var deltaX = (rect.Width - _text.Width) / 2;
        var deltaY = (rect.Height - _text.Height) / 2;
            
        context.DrawText(_text, new Point(rect.X + deltaX * (int)ha, rect.Y + deltaY * (int)va));
    }

    public static Brush GetBrush(InwardBrushType type, ICellGameDrawingData data)
    {
        return type switch
        {
            InwardBrushType.Cursor => data.CursorBrush,
            InwardBrushType.Link => data.LinkBrush,
            _ => throw new Exception()
        };
    }

    public static (double, double) OrderByAndAddToLast(double one, double two, double toAdd)
    {
        return one < two ? (one, two + toAdd) : (two, one + toAdd);

    }
}

public interface IDefaultDrawingData
{
    double Width { get; }
    double Height { get; }
    
    Typeface Typeface { get; }
    CultureInfo CultureInfo { get; }
}

public interface ICellGameDrawingData : IDefaultDrawingData
{
    Brush CursorBrush { get; }
    Brush LinkBrush { get; }
    
    double InwardCellLineWidth { get; }
    double CellSize { get; }
    double LinkOffset { get; }
    LinkOffsetSidePriority LinkOffsetSidePriority { get; }
    
    double GetLeftOfCell(int col);
    double GetTopOfCell(int row);
    bool IsClue(int row, int col);
}

public interface ICellAndNumbersGameDrawingData : ICellGameDrawingData
{
    Brush DefaultNumberBrush { get; }
    Brush ClueNumberBrush { get; }
}

public interface INinePossibilitiesGameDrawingData : ICellAndNumbersGameDrawingData
{
    bool FastPossibilityDisplay { get; }
    double InwardPossibilityLineWidth { get; }
    
    double GetLeftOfPossibility(int col, int possibility);
    double GetTopOfPossibility(int row, int possibility);
    Point GetCenterOfPossibility(int row, int col, int possibility);
}

public class InwardCellDrawableComponent : IDC<ICellGameDrawingData>
{
    private readonly int _row;
    private readonly int _col;
    private readonly InwardBrushType _brushType;

    public InwardCellDrawableComponent(int row, int col, InwardBrushType brushType)
    {
        _row = row;
        _col = col;
        _brushType = brushType;
    }

    public void Draw(DrawingContext context, ICellGameDrawingData data)
    {
        var delta = data.InwardCellLineWidth / 2;
        var left = data.GetLeftOfCell(_col);
        var top = data.GetTopOfCell(_row);
        var pen = new Pen(DCHelper.GetBrush(_brushType, data), data.InwardCellLineWidth);

        context.DrawLine(pen, new Point(left + delta, top), new Point(left + delta,
            top + data.CellSize));
        context.DrawLine(pen, new Point(left, top + delta), new Point(left + data.CellSize,
            top + delta));
        context.DrawLine(pen, new Point(left + data.CellSize - delta, top),
            new Point(left + data.CellSize - delta, top + data.CellSize));
        context.DrawLine(pen, new Point(left, top + data.CellSize - delta), 
            new Point(left + data.CellSize, top + data.CellSize - delta));
    }
}

public class InwardPossibilityDrawableComponent : IDC<INinePossibilitiesGameDrawingData>
{
    private readonly int _possibility;
    private readonly int _row;
    private readonly int _col;
    private readonly InwardBrushType _brushType;

    public InwardPossibilityDrawableComponent(int possibility, int row, int col, InwardBrushType brushType)
    {
        _possibility = possibility;
        _row = row;
        _col = col;
        _brushType = brushType;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        var pSize = data.CellSize / 3;
        var delta = data.InwardPossibilityLineWidth / 2;
        var left = data.GetLeftOfPossibility(_col, _possibility);
        var top = data.GetTopOfPossibility(_row, _possibility);
        var pen = new Pen(DCHelper.GetBrush(_brushType, data), data.InwardPossibilityLineWidth);

        context.DrawLine(pen, new Point(left + delta, top), new Point(left + delta,
            top + pSize));
        context.DrawLine(pen, new Point(left, top + delta), new Point(left + pSize,
            top + delta));
        context.DrawLine(pen, new Point(left + pSize - delta, top),
            new Point(left + pSize - delta, top + pSize));
        context.DrawLine(pen, new Point(left, top + pSize - delta), 
            new Point(left + pSize, top + pSize - delta));
    }
}

public enum InwardBrushType
{
    Cursor, Link
}

public class SolutionDrawableComponent : IDC<ICellAndNumbersGameDrawingData>
{
    private readonly int _solution;
    private readonly int _row;
    private readonly int _col;

    public SolutionDrawableComponent(int solution, int row, int col)
    {
        _solution = solution;
        _row = row;
        _col = col;
    }

    public void Draw(DrawingContext context, ICellAndNumbersGameDrawingData data)
    {
        var brush = data.IsClue(_row, _col) ? data.ClueNumberBrush : data.DefaultNumberBrush;
        var text = new FormattedText(_solution.ToString(), data.CultureInfo, FlowDirection.LeftToRight, data.Typeface,
            data.CellSize / 4 * 3, brush, 1);
        DCHelper.DrawTextInRectangle(context, text, new Rect(data.GetLeftOfCell(_col),
            data.GetTopOfCell(_row), data.CellSize, data.CellSize), ComponentHorizontalAlignment.Center,
            ComponentVerticalAlignment.Center);
    }
}

public class NinePossibilitiesDrawableComponent : IDC<INinePossibilitiesGameDrawingData>
{
    private readonly IEnumerable<int> _possibilities;
    private readonly int _row;
    private readonly int _col;

    public NinePossibilitiesDrawableComponent(IEnumerable<int> possibilities, int row, int col)
    {
        _possibilities = possibilities;
        _row = row;
        _col = col;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        if (data.FastPossibilityDisplay)
        {
            StringBuilder builder = new();
            for (int i = 0; i < 9; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    var n = i + j + 1;
                    builder.Append(_possibilities.Contains(n) ? (char)('0' + n) : ' ');
                    if (j < 2) builder.Append(' ');
                }

                if (i < 6) builder.Append('\n');
            }
            
            var text = new FormattedText(builder.ToString(), data.CultureInfo, FlowDirection.LeftToRight, data.Typeface,
                data.CellSize / 4, data.DefaultNumberBrush, 1);
            DCHelper.DrawTextInRectangle(context, text, new Rect(data.GetLeftOfCell(_col), 
                data.GetTopOfCell(_row), data.CellSize, data.CellSize), ComponentHorizontalAlignment.Center,
                ComponentVerticalAlignment.Center);
        }
        else
        {
            foreach (var possibility in _possibilities)
            {
                var text = new FormattedText(possibility.ToString(), data.CultureInfo, FlowDirection.LeftToRight,
                    data.Typeface, data.CellSize / 4, data.DefaultNumberBrush, 1);
                var pSize = data.CellSize / 3;
                DCHelper.DrawTextInRectangle(context, text, new Rect(data.GetLeftOfPossibility(_col, possibility),
                    data.GetTopOfPossibility(_row, possibility), pSize, pSize), ComponentHorizontalAlignment.Center,
                    ComponentVerticalAlignment.Center);
            }
        }
    }
}

public class CellRectangleDrawableComponent : IDC<ICellGameDrawingData>
{
    private readonly int _rowFrom;
    private readonly int _colFrom;
    private readonly int _rowTo;
    private readonly int _colTo;
    private readonly StepColor _color;

    public CellRectangleDrawableComponent(int rowFrom, int colFrom, int rowTo, int colTo, StepColor color)
    {
        _rowFrom = rowFrom;
        _colFrom = colFrom;
        _rowTo = rowTo;
        _colTo = colTo;
        _color = color;
    }

    public void Draw(DrawingContext context, ICellGameDrawingData data)
    {
        var delta = data.InwardCellLineWidth / 2;
        
        var xFrom = data.GetLeftOfCell(_colFrom) - delta;
        var yFrom = data.GetTopOfCell(_rowFrom) - delta;
        
        var xTo = data.GetLeftOfCell(_colTo) - delta;
        var yTo = data.GetTopOfCell(_rowTo) - delta;

        var toAdd = data.CellSize + data.InwardCellLineWidth;
        var (leftX, rightX) = DCHelper.OrderByAndAddToLast(xFrom, xTo, toAdd);
        var (topY, bottomY) = DCHelper.OrderByAndAddToLast(yFrom, yTo, toAdd);
        
        context.DrawRectangle(null, new Pen(App.Current.ThemeInformation.ToBrush(_color), data.InwardCellLineWidth),
            new Rect(new Point(leftX, topY), new Point(rightX, bottomY)));
    }
}

public class PossibilityRectangleDrawableComponent : IDC<INinePossibilitiesGameDrawingData>
{
    private readonly int _rowFrom;
    private readonly int _colFrom;
    private readonly int _possibilityFrom;
    private readonly int _rowTo;
    private readonly int _colTo;
    private readonly int _possibilityTo;
    private readonly StepColor _color;

    public PossibilityRectangleDrawableComponent(int rowFrom, int colFrom, int possibilityFrom, int rowTo,
        int colTo, int possibilityTo, StepColor color)
    {
        _rowFrom = rowFrom;
        _colFrom = colFrom;
        _possibilityFrom = possibilityFrom;
        _rowTo = rowTo;
        _colTo = colTo;
        _possibilityTo = possibilityTo;
        _color = color;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        var delta = data.InwardPossibilityLineWidth / 2;
        
        var xFrom = data.GetLeftOfPossibility(_colFrom, _possibilityFrom) - delta;
        var yFrom = data.GetTopOfPossibility(_rowFrom, _possibilityFrom) - delta;
        
        var xTo = data.GetLeftOfPossibility(_colTo, _possibilityTo) - delta;
        var yTo = data.GetTopOfPossibility(_rowTo, _possibilityTo) - delta;

        var toAdd = data.CellSize / 3 + data.InwardPossibilityLineWidth;
        var (leftX, rightX) = DCHelper.OrderByAndAddToLast(xFrom, xTo, toAdd);
        var (topY, bottomY) = DCHelper.OrderByAndAddToLast(yFrom, yTo, toAdd);
        
        context.DrawRectangle(null, new Pen(App.Current.ThemeInformation.ToBrush(_color), data.InwardPossibilityLineWidth),
            new Rect(new Point(leftX, topY), new Point(rightX, bottomY)));
    }
}

public class PossibilityPatchDrawableComponent : IDC<INinePossibilitiesGameDrawingData>
{
    private readonly CellPossibility[] _cps;
    private readonly StepColor _color;

    public PossibilityPatchDrawableComponent(CellPossibility[] cps, StepColor color)
    {
        _cps = cps;
        _color = color;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        var brush = App.Current.ThemeInformation.ToBrush(_color);
        var pSize = data.CellSize / 3;
        
        foreach (var cp in _cps)
        {
            var left = data.GetLeftOfPossibility(cp.Column, cp.Possibility);
            var top = data.GetTopOfPossibility(cp.Row, cp.Possibility);

            if(!_cps.ContainsAdjacent(cp, 0, -1)) context.DrawRectangle
                (brush, null, new Rect(left, top, pSize, data.InwardPossibilityLineWidth));
            
            if(!_cps.ContainsAdjacent(cp, -1, 0)) context.DrawRectangle
                (brush, null, new Rect(left, top, data.InwardPossibilityLineWidth, pSize));
            else
            {
                if(_cps.ContainsAdjacent(cp, 0, -1) && !_cps.ContainsAdjacent(cp, -1, -1)) context.DrawRectangle
                    (brush, null, new Rect(left, top, data.InwardPossibilityLineWidth, data.InwardPossibilityLineWidth));
                
                if(_cps.ContainsAdjacent(cp, 0, 1) && !_cps.ContainsAdjacent(cp, -1, 1)) context.DrawRectangle
                    (brush, null, new Rect(left, top + pSize - data.InwardPossibilityLineWidth,
                        data.InwardPossibilityLineWidth, data.InwardPossibilityLineWidth));
            }
            
            if(!_cps.ContainsAdjacent(cp, 0, 1)) context.DrawRectangle
                (brush, null, new Rect(left, top + pSize - data.InwardPossibilityLineWidth, pSize, data.InwardPossibilityLineWidth));
            
            if(!_cps.ContainsAdjacent(cp, 1, 0)) context.DrawRectangle
                (brush, null, new Rect(left + pSize - data.InwardPossibilityLineWidth, top, data.InwardPossibilityLineWidth, pSize));
            else
            {
                if (_cps.ContainsAdjacent(cp, 0, -1) && !_cps.ContainsAdjacent(cp, 1, -1))
                    context.DrawRectangle(brush, null, new Rect(
                        left + pSize - data.InwardPossibilityLineWidth, top, data.InwardPossibilityLineWidth, 
                        data.InwardPossibilityLineWidth));

                if (_cps.ContainsAdjacent(cp, 0, 1) && !_cps.ContainsAdjacent(cp, 1, 1))
                    context.DrawRectangle(brush, null, new Rect(left + pSize - data.InwardPossibilityLineWidth,
                            top + pSize - data.InwardPossibilityLineWidth, data.InwardPossibilityLineWidth, data.InwardPossibilityLineWidth));
            }
        }
    }
}

public class PossibilityLinkDrawableComponent : IDC<INinePossibilitiesGameDrawingData>
{
    private readonly int _rowFrom;
    private readonly int _colFrom;
    private readonly int _possibilityFrom;
    private readonly int _rowTo;
    private readonly int _colTo;
    private readonly int _possibilityTo;
    private readonly LinkStrength _link;

    public PossibilityLinkDrawableComponent(int rowFrom, int colFrom, int possibilityFrom, int rowTo, 
        int colTo, int possibilityTo, LinkStrength link)
    {
        _rowFrom = rowFrom;
        _colFrom = colFrom;
        _possibilityFrom = possibilityFrom;
        _rowTo = rowTo;
        _colTo = colTo;
        _possibilityTo = possibilityTo;
        _link = link;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        var from = data.GetCenterOfPossibility(_rowFrom, _colFrom, _possibilityFrom);
        var to = data.GetCenterOfPossibility(_rowTo, _colTo, _possibilityTo);
        var middle = new Point(from.X + (to.X - from.X) / 2, from.Y + (to.Y - from.Y) / 2);

        var offsets = MathUtility.ShiftSecondPointPerpendicularly(from, middle, data.LinkOffset);

        var validOffsets = new List<Point>();
        for (int i = 0; i < 2; i++)
        {
            var p = offsets[i];
            if(p.X > 0 && p.X < data.Width && p.Y > 0 && p.Y < data.Height) validOffsets.Add(p);
        }

        bool isWeak = _link == LinkStrength.Weak;
        switch (validOffsets.Count)
        {
            case 0 : 
                AddShortenedLine(context, data, from, to, isWeak);
                break;
            case 1 :
                AddShortenedLine(context, data, from, validOffsets[0], to, isWeak);
                break;
            case 2 :
                if(data.LinkOffsetSidePriority == LinkOffsetSidePriority.Any) 
                    AddShortenedLine(context, data, from, validOffsets[0], to, isWeak);
                else
                {
                    var left = MathUtility.IsLeft(from, to, validOffsets[0]) ? 0 : 1;
                    AddShortenedLine(context, data, from, data.LinkOffsetSidePriority == LinkOffsetSidePriority.Left 
                        ? validOffsets[left] 
                        : validOffsets[(left + 1) % 2], to, isWeak);
                }
                break;
        }
    }
    
    private static void AddShortenedLine(DrawingContext context, INinePossibilitiesGameDrawingData data,
        Point from, Point to, bool isWeak)
    {
        var shortening = data.CellSize / 6;

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var mag = Math.Sqrt(dx * dx + dy * dy);
        var newFrom = new Point(from.X + shortening * dx / mag, from.Y + shortening * dy / mag);
        var newTo = new Point(to.X - shortening * dx / mag, to.Y - shortening * dy / mag);
        
        AddLine(context, data, newFrom, newTo, isWeak);
    }
    
    private static void AddShortenedLine(DrawingContext context, INinePossibilitiesGameDrawingData data,
        Point from, Point middle, Point to, bool isWeak)
    {
        var shortening = data.CellSize / 6;
        
        var dxFrom = middle.X - from.X;
        var dyFrom = middle.Y - from.Y;
        var mag = Math.Sqrt(dxFrom * dxFrom + dyFrom * dyFrom);
        var newFrom = new Point(from.X + shortening * dxFrom / mag, from.Y + shortening * dyFrom / mag);

        var dxTo = to.X - middle.X;
        var dyTo = to.Y - middle.Y;
        mag = Math.Sqrt(dxTo * dxTo + dyTo * dyTo);
        var newTo = new Point(to.X - shortening * dxTo / mag, to.Y - shortening * dyTo / mag);
            
        AddLine(context, data, newFrom, middle, isWeak);
        AddLine(context, data,middle, newTo, isWeak);
    }

    private static void AddLine(DrawingContext context, INinePossibilitiesGameDrawingData data,
        Point from, Point to, bool isWeak)
    {
        context.DrawLine(new Pen(data.LinkBrush, 2)
        {
            DashStyle = isWeak ? DashStyles.DashDot : DashStyles.Solid
        }, from, to);
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

    public void Draw(DrawingContext context, bool withGuidelines)
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

public class FilledPolygonComponent : IDrawableComponent
{
    private readonly Point[] _points;
    private Brush _brush;

    public FilledPolygonComponent(Brush brush, params Point[] points)
    {
        _brush = brush;
        _points = points;
    }

    public FilledPolygonComponent(Brush brush, IEnumerable<Point> points)
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
    protected static readonly Typeface Typeface =
        new(new FontFamily(new Uri("pack://application:,,,/View/Fonts/"), "./#Roboto Mono"),
        FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);
    
    protected FormattedText _text;
    protected readonly Rect _rect;
    protected readonly ComponentHorizontalAlignment _horizontalAlignment;
    protected readonly ComponentVerticalAlignment _verticalAlignment;

    public string Text => _text.Text;

    public TextInRectangleComponent(string text, double size, Brush foreground, Rect rect,
        ComponentHorizontalAlignment horizontalAlignment, ComponentVerticalAlignment verticalAlignment)
    {
        _text = new FormattedText(text, Info, Flow, Typeface, size, foreground, 1);
        _rect = rect;
        _horizontalAlignment = horizontalAlignment;
        _verticalAlignment = verticalAlignment;
    }

    protected TextInRectangleComponent(TextInRectangleComponent component)
    {
        _text = component._text;
        _rect = component._rect;
        _horizontalAlignment = component._horizontalAlignment;
        _verticalAlignment = component._verticalAlignment;
    }

    public void SetForegroundFor(Brush brush, char letter)
    {
        for (int i = 0; i < _text.Text.Length; i++)
        {
            if(_text.Text[i] == letter) _text.SetForegroundBrush(brush, i, 1);
        }
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

public class OutlinedTextInRectangleComponent : TextInRectangleComponent
{
    private readonly Brush _borderBrush;
    private readonly double _strokeWidth;
    private readonly List<int> _indexes = new();
    
    public OutlinedTextInRectangleComponent(string text, double size, Brush foreground, Rect rect,
        ComponentHorizontalAlignment horizontalAlignment, ComponentVerticalAlignment verticalAlignment,
        Brush borderBrush, double strokeWidth, char letter)
        : base(text, size, foreground, rect, horizontalAlignment, verticalAlignment)
    {
        _borderBrush = borderBrush;
        _strokeWidth = strokeWidth;
        
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == letter) _indexes.Add(i);
        }
    }

    public OutlinedTextInRectangleComponent(TextInRectangleComponent baseComponent, Brush borderBrush,
        double strokeWidth, char letter) : base(baseComponent)
    {
        _borderBrush = borderBrush;
        _strokeWidth = strokeWidth;
        
        for (int i = 0; i < baseComponent.Text.Length; i++)
        {
            if (baseComponent.Text[i] == letter) _indexes.Add(i);
        }
    }

    public override void Draw(DrawingContext context, bool withGuidelines)
    {
        var deltaX = (_rect.Width - _text.Width) / 2;
        var deltaY = (_rect.Height - _text.Height) / 2;

        base.Draw(context, withGuidelines);
        
        var point = new Point(_rect.X + deltaX * (int)_horizontalAlignment,
            _rect.Y + deltaY * (int)_verticalAlignment);
        foreach (var index in _indexes)
        {
            var geometry = _text.BuildHighlightGeometry(point, index, 1);
            context.DrawGeometry(null, new Pen(_borderBrush, _strokeWidth), geometry);
        }
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