namespace Model.Utility;

public readonly struct RGB
{
    public RGB(byte red, byte green, byte blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    public byte Red { get; }
    public byte Green { get; }
    public byte Blue { get; }

    public int ToHex()
    {
        return (Red << 16) | (Green << 8) | Blue;
    }

    public static RGB FromHex(int hex)
    {
        return new RGB((byte)(hex >> 16), (byte)(hex >> 8), (byte)hex);
    }
}