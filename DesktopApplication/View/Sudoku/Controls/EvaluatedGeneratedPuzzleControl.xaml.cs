using System;
using System.Globalization;
using System.Windows.Media;
using DesktopApplication.View.Utility;
using Model.Helpers.Logs;
using Model.Sudoku.Generator;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class EvaluatedGeneratedPuzzleControl
{
    public EvaluatedGeneratedPuzzleControl(EvaluatedGeneratedPuzzle puzzle)
    {
        InitializeComponent();

        Sudoku.Text = puzzle.Sudoku;
        Rating.Text = Math.Round(puzzle.Rating, 2).ToString(CultureInfo.CurrentCulture);
        if (puzzle.HardestStrategy is null)
        {
            Hardest.Text = string.Empty;
        }
        else
        {
            Hardest.Text = puzzle.HardestStrategy.Name;
            Hardest.Foreground =
                new SolidColorBrush(ColorUtility.ToColor((Intensity)puzzle.HardestStrategy.Difficulty));
        }
    }
}