using System.Windows.Controls;

namespace View.Canvas;

public abstract class OptionCanvas : UserControl
{
    protected bool ShouldCallSetter { get; set; }
    
    public abstract string Explanation { get; }
    public abstract void SetFontSize(int size);
    public abstract void InternalRefresh();

    public void Refresh()
    {
        ShouldCallSetter = false;
        InternalRefresh();
        ShouldCallSetter = true;
    }
}