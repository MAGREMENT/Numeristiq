using System;

namespace Model.Utility;

public class Randomizer
{
    protected readonly Random _random = new();

    public bool GenerateChance(int n, int over)
    {
        return _random.Next(over) < n;
    }

    public int GenerateBetween(int min, int max) => _random.Next(min, max);
}