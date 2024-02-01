namespace Model.Utility;

public readonly struct MinMax
{
    public MinMax(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public static MinMax From(string s)
    {
        var i = s.IndexOf(',');
        if (i == -1) return new MinMax(0, 0);

        try
        {
            return new MinMax(int.Parse(s[..i]), int.Parse(s[(i + 1)..]));
        }
        catch
        {
            return new MinMax(0, 0);
        }
    }

    public override string ToString()
    {
        return $"{Min},{Max}";
    }

    public int Min { get; }
    public int Max { get; }
}