using System.Windows.Media;
using Global.Enums;

namespace View.Utility;

public class ColorManager
{
    public static readonly Brush Background1 = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
    public static readonly Brush Background2 = new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xEF));
    public static readonly Brush Background3 = new SolidColorBrush(Color.FromRgb(0xD1, 0xD1, 0xD1));
    public static readonly Brush Green = new SolidColorBrush(Color.FromRgb(0x50, 0xC1, 0x31));
    public static readonly Brush Purple = new SolidColorBrush(Color.FromRgb(0xBA, 0x7B, 0xC8));

    public static readonly Brush[] CellBrushes =
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

    public static Brush ToBrush(CellColor cc)
    {
        return CellBrushes[(int)cc];
    }

    

    private static ColorManager? _instance;

    public static ColorManager GetInstance()
    {
        _instance ??= new ColorManager();
        return _instance;
    }

    private ColorManager()
    {
    }
}