using System.Windows;
using System.Windows.Controls;
using Model.Utility.Collections;

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

    public NotifyingList<FrameworkElement> SideControls { get; } = new();
    
    public SizeOptimizedContentControl()
    {
        InitializeComponent();
        
        NoSize.SetResourceReference(ForegroundProperty, "Text");
        SizeChanged += (_, _) => AdjustOptimizableSize();

        SideControls.ElementAdded += e =>
        {
            if (SidePanel.Children.Count == 0) SidePanel.Margin = new Thickness(10, 0, 0, 0);
            SidePanel.Children.Add(e);
        };
    }

    private void AdjustOptimizableSize()
    {
        if (_content is null || !_content.HasSize())
        {
            SetContent(NoSize);
            return;
        }

        if (ActualWidth is not double.NaN and > 0 && ActualHeight is not double.NaN and > 0)
        {
            var sidePanelWidth = SidePanel.ActualWidth is double.NaN ? 0 : SidePanel.ActualWidth;
            var availableWidth = ActualWidth - OuterBorder.Padding.Left - OuterBorder.Padding.Right
                - sidePanelWidth - SidePanel.Margin.Left - SidePanel.Margin.Right;
            var availableHeight = ActualHeight - OuterBorder.Padding.Top - OuterBorder.Padding.Bottom;

            var w = _content.GetWidthSizeMetricFor(availableWidth);
            var h = _content.GetHeightSizeMetricFor(availableHeight);

            _content.SetSizeMetric(w > h ? h : w);
        }
        
        SetContent((FrameworkElement)_content);
    }

    private void SetContent(UIElement element)
    {
        if(ContentHolder.Children.Count > 1) ContentHolder.Children.RemoveRange(1, ContentHolder.Children.Count - 1);
        Grid.SetColumn(element, 0);
        ContentHolder.Children.Add(element);
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