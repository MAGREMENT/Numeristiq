using System.Windows;
using System.Windows.Controls;

namespace DesktopApplication.View.Controls;

public partial class SizeOptimizedContentControl
{
    private readonly TextBlock NoSize = new()
    {
        FontSize = 22,
        FontWeight = FontWeights.SemiBold,
        Text = "Nothing to show...",
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center
    };

    private ISizeOptimizable? _content;

    public ISizeOptimizable? OptimizableContent
    {
        get => _content;
        set
        {
            if (_content is not null) _content.OptimizableSizeChanged -= AdjustOptimizableSize;
            _content = value;

            if (_content is FrameworkElement e)
            {
                e.VerticalAlignment = VerticalAlignment.Center;
                e.HorizontalAlignment = HorizontalAlignment.Center;
                AdjustOptimizableSize();
                _content.OptimizableSizeChanged += AdjustOptimizableSize;
            }
        }
    }
    
    public SizeOptimizedContentControl()
    {
        InitializeComponent();
        
        NoSize.SetResourceReference(ForegroundProperty, "Text");
        SizeChanged += (_, _) => AdjustOptimizableSize();
    }

    private void AdjustOptimizableSize()
    {
        if (_content is null || !_content.HasSize())
        {
            ContentHolder.Child = NoSize;
            return;
        }

        if (ActualWidth is not double.NaN and > 0 && ActualHeight is not double.NaN and > 0)
        {
            var availableWidth = ActualWidth - ContentHolder.Padding.Left - ContentHolder.Padding.Right;
            var availableHeight = ActualHeight - ContentHolder.Padding.Top - ContentHolder.Padding.Bottom;

            var w = _content.GetWidthSizeMetricFor(availableWidth);
            var h = _content.GetHeightSizeMetricFor(availableHeight);

            _content.SetSizeMetric(w > h ? h : w);
        }
        
        ContentHolder.Child = (FrameworkElement)_content;
    }
}

public interface ISizeOptimizable
{
    event OnSizeChange? OptimizableSizeChanged;
    
    double GetWidthSizeMetricFor(double space);
    double GetHeightSizeMetricFor(double space);
    
    bool HasSize();
    void SetSizeMetric(double n);
}

public delegate void OnSizeChange();