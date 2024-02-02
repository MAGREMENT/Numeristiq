namespace Model.Utility;

public readonly struct MinMax
{
    public MinMax(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public override string ToString()
    {
        return $"{Min},{Max}";
    }

    public int Min { get; }
    public int Max { get; }
}