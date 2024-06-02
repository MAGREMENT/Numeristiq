using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.View.HelperWindows.Dialog;
using DesktopApplication.View.Utility;
using Model;
using Model.Sudokus.Generator;

namespace DesktopApplication.View.Sudokus.Controls;

public partial class GeneratedPuzzleControl
{
    private readonly GeneratedSudokuPuzzle _puzzle;
    
    public GeneratedPuzzleControl(GeneratedSudokuPuzzle puzzle)
    {
        InitializeComponent();
        _puzzle = puzzle;

        Id.Text = $"#{puzzle.Id}";

        if (puzzle.Evaluated)
        {
            Rating.Visibility = Visibility.Visible;
            Hardest.Visibility = Visibility.Visible;
            Rating.Text = Math.Round(puzzle.Rating, 2).ToString(CultureInfo.CurrentCulture);
            if (puzzle.Hardest is null)
            {
                Hardest.Text = string.Empty;
            }
            else
            {
                Hardest.Text = puzzle.Hardest.Name;
                Hardest.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(puzzle.Hardest.Difficulty));
            }
        }
    }

    private void Copy(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_puzzle.AsString());
    }

    private void Show(object sender, RoutedEventArgs e)
    {
        var dialog = new ShowSudokuDialog(_puzzle.Sudoku);
        dialog.Show();
    }
}