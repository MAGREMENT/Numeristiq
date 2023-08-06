using System;
using System.Windows.Media;
using Model;
using Model.Logs;

namespace SudokuSolver;

public static class ColorUtil
{
    public static Color ToColor(ChangeColoration coloration)
    {
        return coloration switch
        {
            ChangeColoration.Change => Colors.Aqua,
            ChangeColoration.CauseOne => Colors.Coral,
            ChangeColoration.CauseTwo => Colors.Red,
            ChangeColoration.CauseThree => Colors.Green,
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
    
    public static Color ToColor(StrategyLevel level)
    {
        return level switch
        {
            StrategyLevel.Basic => Colors.RoyalBlue,
            StrategyLevel.Easy => Colors.Green,
            StrategyLevel.Medium => Colors.Orange,
            StrategyLevel.Hard => Colors.Red,
            StrategyLevel.Extreme => Colors.Purple,
            StrategyLevel.ByTrial => Colors.Black,
            _ => Colors.Gray
        };
    }
}