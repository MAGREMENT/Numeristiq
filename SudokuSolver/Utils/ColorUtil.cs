using System.Windows.Media;
using Model.Solver;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Helpers.Logs;

namespace SudokuSolver.Utils;

public static class ColorUtil
{
    public static readonly Brush Background1 = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
    public static readonly Brush Background2 = new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xEF));
    public static readonly Brush Background3 = new SolidColorBrush(Color.FromRgb(0xD1, 0xD1, 0xD1));
    public static readonly Brush Green = new SolidColorBrush(Color.FromRgb(0x50, 0xC1, 0x31));
    public static readonly Brush Purple = new SolidColorBrush(Color.FromRgb(0xBA, 0x7B, 0xC8));

    public static Color ToColor(ChangeColoration coloration)
    {
        return coloration switch
        {
            ChangeColoration.ChangeOne => Colors.RoyalBlue,
            ChangeColoration.ChangeTwo => Colors.CornflowerBlue,
            ChangeColoration.CauseOffOne => Colors.Coral,
            ChangeColoration.CauseOffTwo => Colors.Red,
            ChangeColoration.CauseOffThree => Colors.Yellow,
            ChangeColoration.CauseOffFour => Colors.Brown,
            ChangeColoration.CauseOffFive => Colors.Chocolate,
            ChangeColoration.CauseOffSix => Colors.Firebrick,
            ChangeColoration.CauseOnOne => Colors.Green,
            ChangeColoration.Neutral => Colors.Silver,
            _ => Colors.White
        };
    }

    public static Color ToColor(Intensity intensity)
    {
        return intensity switch
        {
            Intensity.Zero => Colors.Gray,
            Intensity.One => Colors.RoyalBlue,
            Intensity.Two => Colors.Green,
            Intensity.Three => Colors.Orange,
            Intensity.Four => Colors.Red,
            Intensity.Five => Colors.Purple,
            Intensity.Six => Colors.Black,
            _ => Colors.Gray
        };
    }
    
    public static Color ToColor(StrategyDifficulty difficulty)
    {
        return difficulty switch
        {
            StrategyDifficulty.Basic => Colors.RoyalBlue,
            StrategyDifficulty.Easy => Colors.Green,
            StrategyDifficulty.Medium => Colors.Orange,
            StrategyDifficulty.Hard => Colors.Red,
            StrategyDifficulty.Extreme => Colors.Purple,
            StrategyDifficulty.ByTrial => Colors.Black,
            _ => Colors.Gray
        };
    }
}