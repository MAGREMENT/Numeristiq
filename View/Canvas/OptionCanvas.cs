using System.Windows.Controls;
using View.Themes;

namespace View.Canvas;

public abstract class OptionCanvas : UserControl, IThemeable
{
    protected bool ShouldCallSetter { get; set; }
    
    public abstract string Explanation { get; }
    public abstract void SetFontSize(int size);
    public abstract void ApplyTheme(Theme theme);
    protected abstract void InternalRefresh();

    public void Refresh()
    {
        ShouldCallSetter = false;
        InternalRefresh();
        ShouldCallSetter = true;
    }

   
}