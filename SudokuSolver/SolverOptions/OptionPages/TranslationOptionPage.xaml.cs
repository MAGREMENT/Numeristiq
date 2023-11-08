using System.Windows.Controls;
using Model;

namespace SudokuSolver.SolverOptions.OptionPages;

public partial class TranslationOptionPage : OptionPage
{
    public TranslationOptionPage()
    {
        InitializeComponent();
    }

    public override string OptionTitle => "Translation";
    
    protected override void InitializeOptions()
    {
        if (OptionHandler is null) return;
        Type.SelectedIndex = (int)OptionHandler.TranslationType;
    }

    private void SelectedTranslationType(object sender, SelectionChangedEventArgs e)
    {
        if (OptionHandler is null || Initializing) return;
        OptionHandler.TranslationType = (SudokuTranslationType)Type.SelectedIndex;
    }
}