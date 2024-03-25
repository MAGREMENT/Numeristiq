using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopApplication.View.Utility;

public static class Extensions
{
    public static bool IsUnderHalfHeight(this TextBlock tb, MouseEventArgs args)
    {
        return args.GetPosition(tb).Y > tb.ActualHeight / 2;
    }
    
    public static bool IsUnderHalfHeight(this TextBlock tb, DragEventArgs args)
    {
        return args.GetPosition(tb).Y > tb.ActualHeight / 2;
    }
}