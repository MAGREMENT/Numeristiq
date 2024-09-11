using System.Windows;
using System.Windows.Media;

namespace DesktopApplication.View;

public class AttachedProperties
{
    public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.RegisterAttached(
        "HoverBackground", typeof(Brush), typeof(AttachedProperties), 
        new FrameworkPropertyMetadata());
    
    public static readonly DependencyProperty ButtonCornerRadiusProperty = DependencyProperty.RegisterAttached(
        "ButtonCornerRadius", typeof(CornerRadius), typeof(AttachedProperties), 
        new FrameworkPropertyMetadata());
    
    public static void SetHoverBackground(DependencyObject element, Brush value)
    {
        element.SetValue(HoverBackgroundProperty, value);
    }

    public static Brush GetHoverBackground(DependencyObject element)
    {
        return (Brush)element.GetValue(HoverBackgroundProperty);
    }

    public static void SetButtonCornerRadius(DependencyObject element, CornerRadius radius) =>
        element.SetValue(ButtonCornerRadiusProperty, radius);

    public static CornerRadius GetButtonCornerRadius(DependencyObject element) =>
        (CornerRadius)element.GetValue(ButtonCornerRadiusProperty);
}