using System.Windows;
using System.Windows.Controls;
using Global.Enums;
using Presenter.Solver;
using View.HelperWindows.Settings.Options;

namespace View.HelperWindows.Settings;

public partial class SolverSettingsWindow
{
    public SolverSettingsWindow(SolverSettings settings)
    {
        InitializeComponent();
        
        var settingsPage = GetSettingsPages(settings);

        foreach (var page in settingsPage)
        {
            var tb = new TextBlock
            {
                Width = Titles.Width,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 5),
                TextAlignment = TextAlignment.Center,
                Text = page.SettingTitle
            };

            tb.MouseLeftButtonDown += (_, _) => ShowSettingsPage(page);

            page.ExplanationToBeShown += s => OptionExplanation.Text = s;

            Titles.Children.Add(tb);
        }

        ShowSettingsPage(settingsPage[0]);
    }

    private void ShowSettingsPage(Page page)
    {
        Page.Content = page;
    }

    private void Finished(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private static SettingsPage[] GetSettingsPages(SolverSettings settings)
    {
        return new SettingsPage[]
        {
            new("General", 
                new MultiChoiceOptionCanvas("Action on keyboard :", "Defines the action to be executed when pushing a numpad key", 
                    (int)settings.ActionOnCellChange, i => settings.ActionOnCellChange =
                    (ChangeType) i, "Remove possibility", "Add solution"),
                new ComboBoxOptionCanvas("Translation type", "Sets the sudoku to text translation type", (int)settings.TranslationType,
                    i => settings.TranslationType = (SudokuTranslationType)i, "With shortcuts",
                    "With 0's", "With .'s"),
                new CheckBoxOptionCanvas("Solo to given", "Decides if a cell with only one possibility should be transformed into a given when pasting",
                    settings.TransformSoloPossibilityIntoGiven, b => settings.TransformSoloPossibilityIntoGiven = b)),
            new("Graphics",
                new SliderOptionCanvas("Delay before", "Sets the delay between showing the start state and the highlight of a log", 2000, 0, 10,
                    settings.DelayBeforeTransition, i => settings.DelayBeforeTransition = i),
                new SliderOptionCanvas("Delay after", "Sets the delay between showing the highlight and the after state of a log", 2000, 0, 10,
                    settings.DelayAfterTransition, i => settings.DelayAfterTransition = i),
                new ColorComboBoxOptionCanvas("Givens color", "Sets the color of the cells of given digits", (int)settings.GivenColor, 
                    i => settings.GivenColor = (CellColor)i),
                new ColorComboBoxOptionCanvas("Solving color", "Sets the color of the cells of digits to be solved", (int)settings.SolvingColor,
                    i => settings.SolvingColor = (CellColor)i),
                new ComboBoxOptionCanvas("Link offset side priority", "Defines which side of a link is prioritized when offsetting its center",
                    (int)settings.SidePriority, i => settings.SidePriority = (LinkOffsetSidePriority)i, 
                    "Any", "Left", "Right"),
                new CheckBoxOptionCanvas("Show same cell links", "Definies if the link between 2 possibilities in the same cell should be shown",
                    settings.ShowSameCellLinks, b => settings.ShowSameCellLinks = b)),
            new("Solver",
                new CheckBoxOptionCanvas("Unique solution", "Adapts the solver depending on the uniqueness of the solution", settings.UniquenessAllowed,
                    b => settings.UniquenessAllowed = b))
        };
    }
}