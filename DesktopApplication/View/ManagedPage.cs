using System.Windows.Controls;

namespace DesktopApplication.View;

public abstract class ManagedPage : Page
{
    public abstract void OnShow();
    public abstract void OnClose();
    public abstract object? TitleBarContent();
}