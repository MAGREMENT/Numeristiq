using System.Windows.Media;
using Global;
using Global.Enums;

namespace View.Utility;

public static class ColorUtility
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
            HighlightColor.Red => Colors.Red,
            HighlightColor.Blue => Colors.RoyalBlue,
            HighlightColor.Green => Colors.Green,
            HighlightColor.Orange => Colors.Orange,
            HighlightColor.Purple => Colors.Purple,
            HighlightColor.Cyan => Colors.Cyan,
            HighlightColor.Yellow => Colors.Yellow,
            _ => Colors.Transparent
        };
    }

    public static Brush ToBrush(RGB rgb)
    {
        return new SolidColorBrush(Color.FromRgb(rgb.Red, rgb.Green, rgb.Blue));
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