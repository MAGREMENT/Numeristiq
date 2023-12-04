using System.Windows.Controls;

namespace View.HelperWindows.Settings;

public abstract class OptionCanvas : UserControl
{
    public abstract string Explanation { get; }
    public abstract void SetFontSize(int size);
}

public delegate void OnChange<in T>(T change);