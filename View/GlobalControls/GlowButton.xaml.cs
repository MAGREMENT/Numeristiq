using System.Windows;
using System.Windows.Media;

namespace View.GlobalControls;

public partial class GlowButton
{
    public event OnClick? Click;

    public new double Width
    {
        set => Border.Width = value - 2 * Border.BorderThickness.Left;
    }
    
    public new double Height
    {
        set => Border.Height = value - 2 * Border.BorderThickness.Top;
    }

    public new Brush Background
    {
        set
        {
            _background = value;
            if (!isHovered) Text.Background = value;
        }
        get => _background;
    }

    public new Brush BorderBrush
    {
        set
        {
            _borderBrush = value;
            if (!isHovered) Border.BorderBrush = value;
        }
        get => _borderBrush;
    }

    public Brush HoverBackground 
    {
        set
        {
            _hoverBackground = value;
            if (isHovered) Text.Background = value;
        }
        get => _hoverBackground;
    }
    
    public Brush HoverBorderBrush
    {
        set
        {
            _hoverBorderBrush = value;
            if (isHovered) Border.BorderBrush = value;
        }
        get => _hoverBorderBrush;
    }


    private bool isHovered;
    private Brush _background;
    private Brush _borderBrush;
    private Brush _hoverBackground;
    private Brush _hoverBorderBrush;
    
    public GlowButton()
    {
        InitializeComponent();

        _background = Text.Background;
        _hoverBackground = Text.Background;
        _borderBrush = Border.BorderBrush;
        _hoverBorderBrush = Border.BorderBrush;
        
        MouseLeftButtonDown += (o, a) => Click?.Invoke(o, a);
        MouseEnter += (_, _) =>
        {
            isHovered = true;
            Text.Background = _hoverBackground;
            Border.BorderBrush = _hoverBorderBrush;
        };
        MouseLeave += (_, _) =>
        {
            isHovered = false;
            Text.Background = _background;
            Border.BorderBrush = _borderBrush;
        };
    }
}

public delegate void OnClick(object? sender, RoutedEventArgs args);