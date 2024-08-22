using System;

namespace Model.Utility;

public readonly struct RGB
{
    public byte Red { get; }
    public byte Green { get; }
    public byte Blue { get; }
    
    public static RGB FromHex(int hex)
    {
        return new RGB((byte)(hex >> 16), (byte)(hex >> 8), (byte)hex);
    }
    
    public RGB(byte red, byte green, byte blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    public RGB WithRed(byte red) => new(red, Green, Blue);
    public RGB WithGreen(byte green) => new(Red, green, Blue);
    public RGB WithBlue(byte blue) => new(Red, Green, blue);

    public int ToHex()
    {
        return (Red << 16) | (Green << 8) | Blue;
    }

    public HSL ToHSL()
    {
        var r = Red / 255.0;
        var g = Green / 255.0;
        var b = Blue / 255.0;
        var max = Math.Max(Math.Max(r, g), b);
        var min = Math.Min(Math.Min(r, g), b);
        var delta = max - min;
        
        int h;
        if (delta == 0) h = 0;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        else if (max == r) h = (int)(60 * ((g - b) / delta + (g < b ? 6 : 0)));
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        else if (max == g) h = (int)(60 * ((b - r) / delta + 2));
        else h = (int)(60 * ((r - g) / delta + 4));

        var l = (max + min) / 2;
        var s = delta == 0 ? 0 : delta / (1 - Math.Abs(2 * l - 1));
        return new HSL(h, s, l);
    }

    public override bool Equals(object? obj)
    {
        return obj is RGB rgb && rgb == this;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Red, Green, Blue);
    }

    public static bool operator ==(RGB left, RGB right)
    {
        return left.Red == right.Red && left.Blue == right.Blue && left.Green == right.Green;
    }

    public static bool operator !=(RGB left, RGB right)
    {
        return left.Red != right.Red || left.Blue != right.Blue || left.Green != right.Green;
    }
}