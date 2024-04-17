using System.Windows;
using System.Windows.Input;

namespace DesktopApplication.View.HelperWindows.Dialog;

public class DialogWindow : Window
{
    protected bool CloseOnDeactivate { get; set; } = true;
    
    protected DialogWindow()
    {
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;

        Deactivated += (_, _) =>
        {
            if(CloseOnDeactivate) Close();
        };

        Loaded += MoveWindowToUnderCursor;
    } 

    private void MoveWindowToUnderCursor(object obj, RoutedEventArgs args)
    {
        var source = PresentationSource.FromVisual(this);
        if(source is null) return;

        var target = source.CompositionTarget;
        if (target is null) return;
        
        var mouse = target.TransformFromDevice.Transform(PointToScreen(Mouse.GetPosition(this)));
        var left = mouse.X - ActualWidth / 2;
        var top = mouse.Y;

        if (left + ActualWidth > SystemParameters.PrimaryScreenWidth)
            left = SystemParameters.PrimaryScreenWidth - ActualWidth;
        else if (left < 0) left = 0;
        
        if (top + ActualHeight > SystemParameters.PrimaryScreenHeight)
            top = SystemParameters.PrimaryScreenHeight - ActualHeight;

        Left = left;
        Top = top;
    }
}