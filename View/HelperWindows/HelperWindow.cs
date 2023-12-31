using System.ComponentModel;
using System.Windows;
using View.Themes;

namespace View.HelperWindows;

public abstract class HelperWindow : Window, IThemeable
{
    private bool _shouldClose;
    
    protected HelperWindow()
    {
        Closing += OnClosing;
    }

    public void ForceClose()
    {
        _shouldClose = true;
        Close();
    }

    private void OnClosing(object? sender, CancelEventArgs args)
    {
        if (_shouldClose) return;
        
        args.Cancel = true;
        Hide();
    }

    public abstract void ApplyTheme(Theme theme);
}