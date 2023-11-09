using System.Windows;
using System.Windows.Controls;
using SudokuSolver.SolverOptions.OptionPages;

namespace SudokuSolver.SolverOptions;

public partial class SolverOptionWindow
{
    public SolverOptionWindow(ISolverOptionHandler handler)
    {
        InitializeComponent();

        OptionPage[] optionPages = { new GeneralOptionPage(), new GraphicsOptionPage(), new SolverOptionPage() };

        foreach (var page in optionPages)
        {
            page.OptionHandler = handler;
            page.Initialize();

            var tb = new TextBlock
            {
                Width = Titles.Width,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 5),
                TextAlignment = TextAlignment.Center,
                Text = page.OptionTitle
            };

            tb.MouseLeftButtonDown += (_, _) => ShowOptionPage(page);

            Titles.Children.Add(tb);
        }

        ShowOptionPage(optionPages[0]);
    }

    private void ShowOptionPage(OptionPage page)
    {
        Page.Content = page;
    }

    private void Finished(object sender, RoutedEventArgs e)
    {
        Close();
    }
}