using System.Collections.Generic;
using System.Windows.Controls;
using View.HelperWindows;
using View.Themes;

namespace View.Pages;

public abstract class HandledPage : Page, IThemeable
{
    private readonly List<HelperWindow> _helperWindows = new();

    protected void AddManagedHelperWindow(HelperWindow window)
    {
        _helperWindows.Add(window);
    }
    
    protected abstract void InternalApplyTheme(Theme theme);
    
    public abstract void OnShow();
    public abstract void OnQuit();
    
    public void ApplyTheme(Theme theme)
    {
        foreach (var window in _helperWindows)
        {
            window.ApplyTheme(theme);
        }
        InternalApplyTheme(theme);
    }
}