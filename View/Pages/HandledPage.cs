using System.Windows.Controls;
using View.Themes;

namespace View.Pages;

public abstract class HandledPage : Page, IThemeable
{
    public abstract void OnShow();
    public abstract void OnQuit();
    public abstract void Apply(Theme theme);
}