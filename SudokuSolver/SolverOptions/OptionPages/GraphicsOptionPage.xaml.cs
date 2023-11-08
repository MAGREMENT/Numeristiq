using System.Windows;

namespace SudokuSolver.SolverOptions.OptionPages;

public partial class GraphicsOptionPage //TODO : colors
{
    public override string OptionTitle => "Graphics";

    public GraphicsOptionPage()
    {
        InitializeComponent();
    }
    
    protected override void InitializeOptions()
    {
        if (OptionHandler is null) return;
        DelayAfterSlider.Value = OptionHandler.DelayAfter;
        DelayBeforeSlider.Value = OptionHandler.DelayBefore;
    }

    private void SetSolverDelayBefore(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.DelayBefore = (int)DelayBeforeSlider.Value;
    }

    private void SetSolverDelayAfter(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.DelayAfter = (int)DelayAfterSlider.Value;
    }
}