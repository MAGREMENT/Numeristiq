using System.Windows.Media;
using Model;
using Model.Helpers.Changes;
using Model.Helpers.Logs;
using Model.Sudoku;
using Model.Sudoku.Player;
using Model.Sudoku.Solver.Explanation;
using Model.Utility;

namespace DesktopApplication.View.Utility;

public static class ColorUtility //To singleton + theme
{
    private static readonly Brush[] CellBrushes =
    {
        Brushes.Black,
        Brushes.Gray,
        Brushes.Red,
        Brushes.Green,
        Brushes.Blue
    };
    
    public static Brush GetCellBrush(CellColor cc)
    {
        return CellBrushes[(int)cc];
    }

    public static Color ToColor(ChangeColoration coloration)
    {
        return coloration switch
        {
            ChangeColoration.ChangeOne => Colors.RoyalBlue,
            ChangeColoration.ChangeTwo => Colors.CornflowerBlue,
            ChangeColoration.CauseOffOne => Colors.Red,
            ChangeColoration.CauseOffTwo => Colors.Coral,
            ChangeColoration.CauseOffThree => Colors.Orange,
            ChangeColoration.CauseOffFour => Colors.Yellow,
            ChangeColoration.CauseOffFive => Colors.Chocolate,
            ChangeColoration.CauseOffSix => Colors.Firebrick,
            ChangeColoration.CauseOffSeven => Colors.Brown,
            ChangeColoration.CauseOffEight => Colors.SaddleBrown,
            ChangeColoration.CauseOffNine => Colors.DarkRed,
            ChangeColoration.CauseOffTen => Colors.RosyBrown,
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
    
    public static Color ToColor(HighlightColor color)
    {
        return color switch
        {
            HighlightColor.First => Colors.Red,
            HighlightColor.Third => Colors.RoyalBlue,
            HighlightColor.Second => Colors.Green,
            HighlightColor.Fifth => Colors.Orange,
            HighlightColor.Fourth => Colors.Purple,
            HighlightColor.Seventh => Colors.Cyan,
            HighlightColor.Sixth => Colors.Yellow,
            _ => Colors.Transparent
        };
    }

    public static Brush ToBrush(ExplanationColor color)
    {
        return color switch
        {
            ExplanationColor.TextDefault => Brushes.White,
            ExplanationColor.Primary => Brushes.Orange,
            ExplanationColor.Secondary => Brushes.Purple,
            _ => Brushes.Black
        };
    }

    public static Brush ToBrush(RGB rgb)
    {
        return new SolidColorBrush(Color.FromRgb(rgb.Red, rgb.Green, rgb.Blue));
    }

    public static Color ToColor(RGB rgb)
    {
        return Color.FromRgb(rgb.Red, rgb.Green, rgb.Blue);
    }

    public static Color[] ToColors(HighlightColor[] colors)
    {
        Color[] result = new Color[colors.Length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = ToColor(colors[i]);
        }

        return result;
    }

    public static Brush ToBrush(CellColor cc)
    {
        return CellBrushes[(int)cc];
    }
}