using System.Windows;
using System.Windows.Input;

namespace DesktopApplication.View.HelperWindows;

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

        Loaded += MoveBottomRightEdgeOfWindowToMousePosition;
    } 

    private void MoveBottomRightEdgeOfWindowToMousePosition(object obj, RoutedEventArgs args)
    {
        var source = PresentationSource.FromVisual(this);
        if(source is null) return;

        var target = source.CompositionTarget;
        if (target is null) return;
        
        var mouse = target.TransformFromDevice.Transform(PointToScreen(Mouse.GetPosition(this)));
        Left = mouse.X - ActualWidth;
        Top = mouse.Y - ActualHeight;
    }
}