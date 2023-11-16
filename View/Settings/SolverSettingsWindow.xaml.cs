using System.Windows;
using System.Windows.Controls;
using Global.Enums;
using View.Settings.Options;

namespace View.Settings;

public partial class SolverSettingsWindow
{
    public SolverSettingsWindow(ISolverOptionHandler handler)
    {
        InitializeComponent();
        
        var settingsPage = GetSettingsPages(handler);

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

    private static SettingsPage[] GetSettingsPages(ISolverOptionHandler handler)
    {
        return new SettingsPage[]
        {
            new("General", 
                new MultiChoiceOptionCanvas("Action on keyboard :", "Defines the action to be executed when pushing a numpad key", 
                    (int)handler.ActionOnKeyboardInput, i => handler.ActionOnKeyboardInput =
                    (ChangeType) i, "Remove possibility", "Add solution"),
                new ComboBoxOptionCanvas("Translation type", "Sets the sudoku to text translation type", (int)handler.TranslationType,
                    i => handler.TranslationType = (SudokuTranslationType)i, "With shortcuts",
                    "With 0's", "With .'s")),
            new("Graphics",
                new SliderOptionCanvas("Delay before", "Sets the delay between showing the start state and the highlight of a log", 2000, 0, 
                    handler.DelayBeforeTransition, i => handler.DelayBeforeTransition = i),
                new SliderOptionCanvas("Delay after", "Sets the delay between showing the highlight and the after state of a log", 2000, 0,
                    handler.DelayAfterTransition, i => handler.DelayAfterTransition = i),
                new ColorComboBoxOptionCanvas("Givens color", "Sets the color of the cells of given digits", 0, 
                    brush => handler.GivenForegroundColor = brush),
                new ColorComboBoxOptionCanvas("Solving color", "Sets the color of the cells of digits to be solved", 0,
                    brush => handler.SolvingForegroundColor = brush)),
            new("Solver",
                new CheckBoxOptionCanvas("Step by step", "Defines whether the solver stops when progress is made or not", handler.StepByStep,
                    b => handler.StepByStep = b),
                new CheckBoxOptionCanvas("Unique solution", "Adapts the solver depending on the uniqueness of the solution", handler.UniquenessAllowed,
                    b => handler.UniquenessAllowed = b),
                new ComboBoxOptionCanvas("On instance found", "Defines the behavior of the solver when an instance of a strategy is found", (int)handler.OnInstanceFound,
                    i => handler.OnInstanceFound = (OnInstanceFound)i, "Default", "Return",
                    "Wait for all", "Choose best", "Customized"))
        };
    }
}