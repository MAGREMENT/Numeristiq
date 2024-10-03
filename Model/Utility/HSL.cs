using System;

namespace Model.Utility;

public readonly struct HSL
{
    public int Hue { get; }
    public double Saturation { get; }
    public double Lightness { get; }
    
    public HSL(int hue, double saturation, double lightness)
    {
        Hue = hue;
        Saturation = saturation;
        Lightness = lightness;
    }

    public HSL WithHue(int hue) => new(hue, Saturation, Lightness);

    public RGB ToRGB()
    {
        var c = (1 - Math.Abs(2 * Lightness - 1)) * Saturation;
        var x = c * (1 - Math.Abs(Hue / 60.0 % 2 - 1));
        var m = Lightness - c / 2;

        double r, g, b;
        switch (Hue)
        {
            case < 60 :
                r = c;
                g = x;
                b = 0;
                break;
            case < 120 :
                r = x;
                g = c;
                b = 0;
                break;
            case < 180 :
                r = 0;
                g = c;
                b = x;
                break;
            case < 240 :
                r = 0;
                g = x;
                b = c;
                break;
            case < 300 :
                r = x;
                g = 0;
                b = c;
                break;
            default :
                r = c;
                g = 0;
                b = x;
                break;
        }

        return new RGB((byte)Math.Round((r + m) * 255), 
            (byte)Math.Round((g + m) * 255), (byte)Math.Round((b + m) * 255));
    }

    public override string ToString()
    {
        return $"{Hue}, {Saturation * 100}%, {Lightness * 100}%";
    }
}