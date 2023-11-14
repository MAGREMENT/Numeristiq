using System.Windows.Controls;

namespace View.Settings;

public abstract class OptionCanvas : UserControl //TODO Finish transitionning from multi option page to options canvas
{
    public abstract string Explanation { get; }
}

public delegate void OnChange<in T>(T change);