using System.Windows;
using System.Windows.Controls;
using Global.Enums;
using Presenter.Player;
using Presenter.Solver;
using View.HelperWindows.Settings.Options;

namespace View.HelperWindows.Settings;

public partial class SettingsWindow
{
    public SettingsWindow(SettingsPage[] settingsPage)
    {
        InitializeComponent();
        
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

        if(settingsPage.Length > 0) ShowSettingsPage(settingsPage[0]);
    }

    public static SettingsWindow From(ISolverSettings settings)
    {
        var pages = new SettingsPage[]
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

        return new SettingsWindow(pages);
    }

    public static SettingsWindow From(IPlayerSettings settings)
    {
        var pages = new SettingsPage[]
        {
            new("General", 
                new CheckBoxOptionCanvas("Solo to given", "Decides if a cell with only one possibility should be transformed into a given when pasting",
                    settings.TransformSoloPossibilityIntoGiven, b => settings.TransformSoloPossibilityIntoGiven = b)),
            new("Graphics",
                new CheckBoxOptionCanvas("Multi-color highlighting", "Enables multiple colors in a single cell", settings.MultiColorHighlighting,
                    b => settings.MultiColorHighlighting = b),
                new ColorComboBoxOptionCanvas("Givens color", "Sets the color of the cells of given digits", (int)settings.GivenColor, 
                    i => settings.GivenColor = (CellColor)i),
                new ColorComboBoxOptionCanvas("Solving color", "Sets the color of the cells of digits to be solved", (int)settings.SolvingColor,
                    i => settings.SolvingColor = (CellColor)i),
                new SliderOptionCanvas("Highlighting start angle", "Defines the starting angle when highlighting a cell", 360, 
                    0, 5, settings.StartAngle, i => settings.StartAngle = i),
                new MultiChoiceOptionCanvas("Highlighting rotation", "Defines the direction of the rotation when highlighting a cell",
                    (int)settings.RotationDirection, i => settings.RotationDirection = (RotationDirection)i, "Clock wise",
                    "Counter clock wise"))
        };

        return new SettingsWindow(pages);
    }

    private void ShowSettingsPage(Page page)
    {
        Page.Content = page;
    }

    private void Finished(object sender, RoutedEventArgs e)
    {
        Close();
    }
}