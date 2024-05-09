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
    }

    private void AdjustOptimizableSize()
    {
        if (Width is double.NaN || Height is double.NaN) return;
        
        if (_content is null || !_content.HasSize())
        {
            ContentHolder.Child = NoSize;
            return;
        }
        
        var availableWidth = Width - ContentHolder.Padding.Left - ContentHolder.Padding.Right;
        var availableHeight = Height - ContentHolder.Padding.Top - ContentHolder.Padding.Bottom;

        _content.SetSizeMetric(availableHeight - _content.Height < availableWidth - _content.Width
            ? ComputeOptimalSize(availableHeight, _content.HeightSizeMetricCount, _content.GetHeightAdditionalSize(), SizeType.Height) 
            : ComputeOptimalSize(availableWidth, _content.WidthSizeMetricCount, _content.GetWidthAdditionalSize(), SizeType.Width));
        ContentHolder.Child = (FrameworkElement)_content;
    }

    private int ComputeOptimalSize(double space, int count, double additionalSize, SizeType type)
    {
        var value = (int)((space - additionalSize) / count);
        var simulation = _content!.SimulateSizeMetric(value, type);
        while (simulation > space)
        {
            var possibleRemoval = (int)((simulation - space) / count);
            value -= possibleRemoval > 0 ? possibleRemoval : 1;
            simulation = _content!.SimulateSizeMetric(value, type);
        }

        return value;
    }
}

public interface ISizeOptimizable
{
    event OnSizeChange? OptimizableSizeChanged;
    
    double Width { get; }
    double Height { get; }
    
    int WidthSizeMetricCount { get; }
    int HeightSizeMetricCount { get; }

    double GetHeightAdditionalSize();
    double GetWidthAdditionalSize();
    
    bool HasSize();
    double SimulateSizeMetric(int n, SizeType type);
    void SetSizeMetric(int n);
}

public enum SizeType
{
    Width, Height
}

public delegate void OnSizeChange();