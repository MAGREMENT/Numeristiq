using System.Windows;
using System.Windows.Controls;
using Model;
using Model.Solver.Helpers.Changes;

namespace View.SolverOptions.OptionPages;

public partial class GeneralOptionPage
{
    public GeneralOptionPage()
    {
        InitializeComponent();
    }

    public override string OptionTitle => "General";
    
    protected override void InitializeOptions()
    {
        if (OptionHandler is null) return;
        Type.SelectedIndex = (int)OptionHandler.TranslationType;
        if (OptionHandler.ActionOnKeyboardInput == ChangeType.Possibility) PossibilityButton.IsChecked = true;
        else SolutionButton.IsChecked = true;
    }

    private void SelectedTranslationType(object sender, SelectionChangedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.TranslationType = (SudokuTranslationType)Type.SelectedIndex;
    }

    private void ActionOnKeyboardIsSolution(object sender, RoutedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.ActionOnKeyboardInput = ChangeType.Solution;
    }

    private void ActionOnKeyboardIsPossibility(object sender, RoutedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.ActionOnKeyboardInput = ChangeType.Possibility;
    }
}