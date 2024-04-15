using System.Windows;
using System.Windows.Media;

namespace DesktopApplication.View;

public class WelcomeBoard : DrawingBoard
{
    public static readonly DependencyProperty TextBrushProperty = DependencyProperty.Register(nameof(TextBrush),
        typeof(Brush), typeof(WelcomeBoard));

    public Brush TextBrush
    {
        get => (Brush)GetValue(TextBrushProperty);
        set => SetValue(TextBrushProperty, value);
    }
    
    public static readonly DependencyProperty PrimaryBrushProperty = DependencyProperty.Register(nameof(PrimaryBrush),
        typeof(Brush), typeof(WelcomeBoard));

    public Brush PrimaryBrush
    {
        get => (Brush)GetValue(PrimaryBrushProperty);
        set => SetValue(PrimaryBrushProperty, value);
    }
    
    public static readonly DependencyProperty SecondaryBrushProperty = DependencyProperty.Register(nameof(SecondaryBrush),
        typeof(Brush), typeof(WelcomeBoard));

    public Brush SecondaryBrush
    {
        get => (Brush)GetValue(SecondaryBrushProperty);
        set => SetValue(SecondaryBrushProperty, value);
    }

    public WelcomeBoard() : base(1)
    {
    }

    public void DrawBase()
    {
        const int delta = 10;
        for (int i = 1; i <= 3; i++)
        {
            Layers[0].Add(new LineComponent(new Point(Width + delta, Height - 30 * i),
                new Point(Width - 30 * i, Height + delta), new Pen
                {
                    Thickness = 3,
                    Brush = i switch
                    {
                        1 => TextBrush,
                        2 => PrimaryBrush,
                        3 => SecondaryBrush,
                        _ => Brushes.Black
                    }
                }));  
        }

        var rect1 = new Rect(140, -10, 50, 50);
        const double angle1 = 15;
        Layers[0].Add(new AngledOutlinedRectangleComponent(rect1, new Pen
        {
            Thickness = 3,
            Brush = TextBrush
        }, angle1));
        Layers[0].Add(new AngledTextInRectangleComponent("1", rect1.Width * 3 / 4,
            PrimaryBrush, rect1, ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center, angle1));

        var rect2 = new Rect(0, 140, 70, 70);
        const double angle2 = -15;
        Layers[0].Add(new AngledOutlinedRectangleComponent(rect2, new Pen
        {
            Thickness = 3,
            Brush = TextBrush
        }, angle2));
        Layers[0].Add(new AngledTextInRectangleComponent("5", rect2.Width * 3 / 4,
            SecondaryBrush, rect2, ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center, angle2));
    }
}