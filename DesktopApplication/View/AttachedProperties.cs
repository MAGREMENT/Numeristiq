using System.Windows;
using System.Windows.Media;

namespace DesktopApplication.View;

public static class AttachedProperties
{
    public static readonly DependencyProperty OverBackgroundProperty = DependencyProperty.RegisterAttached(
        "OverBackground", typeof(Brush), typeof(AttachedProperties));
    
    public static void SetOverBackground(DependencyObject element, Brush value)
    {
        element.SetValue(OverBackgroundProperty, value);
    }

    public static Brush GetOverBackground(DependencyObject element)
    {
        return (Brush)element.GetValue(OverBackgroundProperty);
    }
}