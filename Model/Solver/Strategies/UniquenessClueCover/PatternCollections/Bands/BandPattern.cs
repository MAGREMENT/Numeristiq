using System.Collections.Generic;
using Global;

namespace Model.Solver.Strategies.UniquenessClueCover.PatternCollections.Bands;

/// <summary>
/// *x  = clue or placement x (must include all clues; can include any or no placements)
/// -Y  = inferred eliminations Y (that required the uniqueness assumption)
/// @  = all values that don't have a star (*) against them
/// </summary>
public abstract class BandPattern
{
    private readonly Dictionary<BoxPosition, int>[] _placements = {new(), new(), new()};
    private readonly Dictionary<BoxPosition, EliminationFlag>[] _eliminations = {new(), new(), new()};

    private readonly Dictionary<BoxPosition, int>[] _placementsBuffer = new Dictionary<BoxPosition, int>[3];
    private readonly Dictionary<BoxPosition, EliminationFlag>[] _eliminationsBuffer =
        new Dictionary<BoxPosition, EliminationFlag>[3];
    
    public int DifferentClueCount { get; }
    public int ClueCount { get; }
    
    protected BandPattern(int differentClueCount, int clueCount)
    {
        DifferentClueCount = differentClueCount;
        ClueCount = clueCount;
    }

    public Dictionary<BoxPosition, int>[] PlacementsWithKey(int[] key)
    {
        for (int i = 0; i < 3; i++)
        {
            _placementsBuffer[i] = _placementsBuffer[key[i]];
        }

        return _placementsBuffer;
    }
}

public readonly struct EliminationFlag
{
    private readonly ushort _n;

    public EliminationFlag(params int[] elimination)
    {
        ushort n = 0;
        foreach (var e in elimination)
        {
            n |= (ushort)(1 << e);
        }

        _n = n;
    }
}

public readonly struct BoxPosition
{
    public BoxPosition(int width, int length)
    {
        Width = width;
        Length = length;
    }

    public int Width { get; }
    public int Length { get; }

    public BoxPosition Transform(int[] widthKey, int[] lengthKey)
    {
        return new BoxPosition(widthKey[Width], lengthKey[Length]);
    }

    public Cell ToCell(int miniNumber, int bandNumber, Unit unit)
    {
        return unit == Unit.Row
            ? new Cell(miniNumber * 3 + Width, bandNumber * 3 + Length)
            : new Cell(bandNumber * 3 + Length, miniNumber * 3 + Width);
    }
}

public static class OrderKeyGenerator
{
    public static IEnumerable<int[]> GenerateAll()
    {
        yield return new[] { 0, 1, 2 };
        yield return new[] { 0, 2, 1 };
        yield return new[] { 1, 0, 2 };
        yield return new[] { 1, 2, 0 };
        yield return new[] { 2, 0, 1 };
        yield return new[] { 2, 1, 0 };
    }
}