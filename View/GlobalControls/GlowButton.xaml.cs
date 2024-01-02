using System.Windows;
using System.Windows.Media;
using View.Themes;

namespace View.GlobalControls;

public partial class GlowButton : IThemeable
{
    public event OnClick? Click;

    public Brush NormalBackground
    {
        set
        {
            _background = value;
            if (!isHovered) Border.Background = value;
        }
        get => _background;
    }

    public Brush NormalBorderBrush
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
            if (isHovered) Border.Background = value;
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

    public string Text
    {
        set => TextBlock.Text = value;
    }

    private bool isHovered;
    private Brush _background;
    private Brush _borderBrush;
    private Brush _hoverBackground;
    private Brush _hoverBorderBrush;
    
    public GlowButton()
    {
        InitializeComponent();

        _background = TextBlock.Background;
        _hoverBackground = TextBlock.Background;
        _borderBrush = Border.BorderBrush;
        _hoverBorderBrush = Border.BorderBrush;
        
        MouseLeftButtonDown += (o, a) => Click?.Invoke(o, a);
        MouseEnter += (_, _) =>
        {
            isHovered = true;
            Border.Background = _hoverBackground;
            Border.BorderBrush = _hoverBorderBrush;
        };
        MouseLeave += (_, _) =>
        {
            isHovered = false;
            Border.Background = _background;
            Border.BorderBrush = _borderBrush;
        };
    }

    public void ApplyTheme(Theme theme)
    {
        TextBlock.Foreground = theme.Text;
        NormalBackground = theme.Background2;
        NormalBorderBrush = theme.Text;
        HoverBackground = theme.Primary2;
        HoverBorderBrush = theme.Primary1;
    }
}

public delegate void OnClick(object? sender, RoutedEventArgs args);