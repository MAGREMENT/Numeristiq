using System.Windows.Controls;
using View.Themes;

namespace View.Canvas;

public abstract class OptionCanvas : UserControl
{
    protected bool ShouldCallSetter { get; set; }
    
    public abstract string Explanation { get; }
    public abstract void SetFontSize(int size);
    protected abstract void InternalRefresh();

    public void Refresh()
    {
        ShouldCallSetter = false;
        InternalRefresh();
        ShouldCallSetter = true;
    }

   
}