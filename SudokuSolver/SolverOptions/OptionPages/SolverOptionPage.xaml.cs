using System.Windows;
using System.Windows.Controls;
using Model.Solver;

namespace SudokuSolver.SolverOptions.OptionPages;

public partial class SolverOptionPage : OptionPage
{
    public override string OptionTitle => "Solver";

    public SolverOptionPage()
    {
        InitializeComponent();
    }
    
    protected override void InitializeOptions()
    {
        if (OptionHandler is null) return;
        StepByStep.IsChecked = OptionHandler.StepByStep;
        Uniqueness.IsChecked = OptionHandler.UniquenessAllowed;
        Box.SelectedIndex = (int)OptionHandler.OnInstanceFound;
    }

    private void AllowUniqueness(object sender, RoutedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.UniquenessAllowed = true;
    }

    private void ForbidUniqueness(object sender, RoutedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.UniquenessAllowed = false;
    }

    private void SelectedOnInstanceFound(object sender, SelectionChangedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.OnInstanceFound = (OnInstanceFound)Box.SelectedIndex;
    }

    private void StepByStepOn(object sender, RoutedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.StepByStep = true;
    }

    private void StepByStepOff(object sender, RoutedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.StepByStep = false;
    }
}