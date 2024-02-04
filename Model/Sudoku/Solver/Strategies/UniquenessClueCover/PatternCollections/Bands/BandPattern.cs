using System.Collections.Generic;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.Possibility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies.UniquenessClueCover.PatternCollections.Bands;

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
    
    public int DifferentClueCount { get; private set; }
    public int ClueCount { get; private set; }

    protected void AddPlacement(int boxNumber, int boxWidth, int boxLength, int number)
    {
        if (!_placements[boxNumber].TryAdd(new BoxPosition(boxWidth, boxLength), number)) return;
        
        ClueCount++;

        var count = 0;
        foreach (var box in _placements)
        {
            if (!box.ContainsValue(number)) continue;
                
            count++;
            if(count > 1) break;
        }

        if (count == 1) DifferentClueCount++;
    }

    protected void AddEliminationFlag(int boxNumber, int boxWidth, int boxLength, EliminationFlag flag)
    {
        _eliminations[boxNumber].TryAdd(new BoxPosition(boxWidth, boxLength), flag);
    }

    public Dictionary<BoxPosition, int>[] PlacementsWithKey(int[] key)
    {
        for (int i = 0; i < 3; i++)
        {
            _placementsBuffer[i] = _placements[key[i]];
        }

        return _placementsBuffer;
    }

    public Dictionary<BoxPosition, EliminationFlag>[] EliminationsWithKey(int[] key)
    {
        for (int i = 0; i < 3; i++)
        {
            _eliminationsBuffer[i] = _eliminations[key[i]];
        }

        return _eliminationsBuffer;
    }
}

public readonly struct EliminationFlag
{
    private readonly ushort _n;
    private readonly bool _reverse;

    public EliminationFlag(bool reverse, params int[] eliminations)
    {
        _reverse = reverse;
        
        ushort n = 0;
        foreach (var e in eliminations)
        {
            n |= (ushort)(1 << e);
        }

        _n = n;
    }

    public IEnumerable<int> EveryElimination(int[] numberEquivalence)
    {
        if (_reverse)
        {
            ReadOnlyBitSet16 result = ReadOnlyBitSet16.Filled(1, 9);
            for (int i = 0; i < 8; i++)
            {
                if (((_n >> i) & 1) > 0) result -= numberEquivalence[i];
            }

            foreach (var p in result.EnumeratePossibilities())
            {
                yield return p;
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                if (((_n >> i) & 1) > 0) yield return numberEquivalence[i];
            }
        }
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

/// <summary>
/// +-------------+-------------+-------------+
/// |  *1   .   . |   .   .   . |   .   .   . |
/// |   .   .   . |  *2   .   . |   .   .   . |
/// |  -@   .   . |  -@   .   . |   .   .   . |
/// +-------------+-------------+-------------+
/// </summary>
public class TwoClueBandPattern : BandPattern
{
    public TwoClueBandPattern()
    {
        AddPlacement(0, 0, 0, 0);
        AddPlacement(1, 1, 0, 1);
        AddEliminationFlag(0, 2, 0, new EliminationFlag(true, 0, 1));
        AddEliminationFlag(1, 2, 0, new EliminationFlag(true, 0, 1));
    }
}

/// <summary>
/// +-------------+-------------+-------------+
/// |  *1   .   . |  *4   .   . |  *5  -3  -3 |
/// |  *2   .   . |  *1 -35 -35 |  *4   .   . |
/// |  *3  -5  -5 |  *2   .   . |  *1   .   . |
/// +-------------+-------------+-------------+
/// </summary>
public class TripleCrossBandPattern : BandPattern
{
    public TripleCrossBandPattern()
    {
        AddPlacement(0, 0, 0, 0);
        AddPlacement(0, 1, 0, 1);
        AddPlacement(0, 2, 0, 2);
        
        AddPlacement(1, 0, 0, 3);
        AddPlacement(1, 1, 0, 0);
        AddPlacement(1, 2, 0, 1);
        
        AddPlacement(2, 0, 0, 4);
        AddPlacement(2, 1, 0, 3);
        AddPlacement(2, 2, 0, 0);
        
        AddEliminationFlag(0, 2, 1, new EliminationFlag(false, 4));
        AddEliminationFlag(0, 2, 2, new EliminationFlag(false, 4));
        
        AddEliminationFlag(1, 1, 1, new EliminationFlag(false, 2, 4));
        AddEliminationFlag(1, 1, 2, new EliminationFlag(false, 2, 4));
        
        AddEliminationFlag(2, 0, 1, new EliminationFlag(false, 2));
        AddEliminationFlag(2, 0, 2, new EliminationFlag(false, 2));
    }
}

/// <summary>
/// +----------+----------+----------+
/// | -2 -2 -2 |  .  . -2 |  .  .  . |
/// |  .  .  . | -2 -2 -2 |  .  . *1 |
/// |  .  .  . |  .  . *1 |  .  . *2 |
/// +----------+----------+----------+
/// </summary>
public class LTripleClueBandPattern : BandPattern
{
    public LTripleClueBandPattern()
    {
        AddPlacement(1, 2, 2, 0);
        AddPlacement(2, 1, 2, 0);
        AddPlacement(2, 2, 2, 1);
        
        AddEliminationFlag(0, 0, 0, new EliminationFlag(false, 1));
        AddEliminationFlag(0, 0, 1, new EliminationFlag(false, 1));
        AddEliminationFlag(0, 0, 2, new EliminationFlag(false, 1));
        AddEliminationFlag(1, 0, 2, new EliminationFlag(false, 1));
        AddEliminationFlag(1, 1, 0, new EliminationFlag(false, 1));
        AddEliminationFlag(1, 1, 1, new EliminationFlag(false, 1));
        AddEliminationFlag(1, 1, 2, new EliminationFlag(false, 1));
    }
}

/// <summary>
/// +----------+----------+----------+
/// | -2 -2 -2 |  .  .  . |  .  .  . |
/// |  .  .  . | -2 -2 -2 |  .  . *1 |
/// |  .  .  . |  .  . *1 |  . *2  . |
/// +----------+----------+----------+
/// </summary>
public class BrokenLTripleClueBandPattern : BandPattern
{
    public BrokenLTripleClueBandPattern()
    {
        AddPlacement(1, 2, 2, 0);
        AddPlacement(2, 1, 2, 0);
        AddPlacement(2, 2, 1, 1);
        
        AddEliminationFlag(0, 0, 0, new EliminationFlag(false, 1));
        AddEliminationFlag(0, 0, 1, new EliminationFlag(false, 1));
        AddEliminationFlag(0, 0, 2, new EliminationFlag(false, 1));
        AddEliminationFlag(1, 1, 0, new EliminationFlag(false, 1));
        AddEliminationFlag(1, 1, 1, new EliminationFlag(false, 1));
        AddEliminationFlag(1, 1, 2, new EliminationFlag(false, 1));
    }
}

/// <summary>
/// +----------+----------+----------+
/// |  .  .  . |  .  . -@ |  .  . *1 |
/// |  .  .  . |  .  .  . |  . *2  . |
/// |  .  .  . |  .  . *1 |  .  .  . |
/// +----------+----------+----------+
/// </summary>
public class DiagonalTripleClueBandPattern : BandPattern
{
    public DiagonalTripleClueBandPattern()
    {
        AddPlacement(2, 0, 2, 0);
        AddPlacement(2, 1, 1, 1);
        AddPlacement(1, 2, 2, 0);
        
        AddEliminationFlag(1, 0, 2, new EliminationFlag(true, 0, 1));
    }
}

/// <summary>
/// +----------+----------+----------+
/// | -1 -1 -1 | -1  .  . |  .  .  . |
/// |  .  .  . |  .  .  . |  .  . *1 |
/// |  .  .  . | -1 *2 *3 |  .  .  . |
/// +----------+----------+----------+
/// </summary>
public class AlmostFlatTTripleClueBandPattern : BandPattern
{
    public AlmostFlatTTripleClueBandPattern()
    {
        AddPlacement(2, 1, 2, 0);
        AddPlacement(1, 2, 1, 1);
        AddPlacement(1, 2, 2, 2);
        
        AddEliminationFlag(0, 0, 0, new EliminationFlag(false, 0));
        AddEliminationFlag(0, 0, 1, new EliminationFlag(false, 0));
        AddEliminationFlag(0, 0, 2, new EliminationFlag(false, 0));
        AddEliminationFlag(1, 0, 0, new EliminationFlag(false, 0));
        AddEliminationFlag(1, 2, 0, new EliminationFlag(false, 0));
    }
}

/// <summary>
/// +----------+----------+----------+
/// |  .  .  . |  .  .  . |  .  . -@ |
/// |  .  .  . |  .  .  . |  .  . *1 |
/// |  .  . *1 |  .  . *2 |  .  .  . |
/// +----------+----------+----------+
/// </summary>
public class ExtendedAlmostFlatTripleClueBandPattern : BandPattern
{
    public ExtendedAlmostFlatTripleClueBandPattern()
    {
        AddPlacement(2, 1, 2, 0);
        AddPlacement(0, 2, 2, 0);
        AddPlacement(1, 2, 2, 1);
        
        AddEliminationFlag(2, 0, 2, new EliminationFlag(true, 0, 1));
    }
}