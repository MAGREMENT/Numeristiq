using System.Collections.Generic;
using Model.Sudokus;
using Model.Sudokus.Solver.Position;
using Model.Tectonics;
using Model.Utility.BitSets;

namespace Model.Utility;

public static class BackTracking //TODO simple fill ?
{
    public static IReadOnlyList<Sudoku> Solutions(Sudoku start, IPossibilitiesGiver giver, int stopAt)
    {
        var result = new BackTrackingListResult<Sudoku>();
        Start(result, start, giver, stopAt);
        return result;
    }

    public static int Count(Sudoku start, IPossibilitiesGiver giver, int stopAt)
    {
        var result = new BackTrackingCountResult<Sudoku>();
        Start(result, start, giver, stopAt);
        return result.Count;
    }
    
    public static IReadOnlyList<ITectonic> Solutions(ITectonic start, IPossibilitiesGiver giver, int stopAt)
    {
        var result = new BackTrackingListResult<ITectonic>();
        Start(result, start, giver, stopAt);
        return result;
    }
    
    public static int Count(ITectonic start, IPossibilitiesGiver giver, int stopAt)
    {
        var result = new BackTrackingCountResult<ITectonic>();
        Start(result, start, giver, stopAt);
        return result.Count;
    }

    private static void Start(IBackTrackingResult<ITectonic> result, ITectonic start, IPossibilitiesGiver giver, int stopAt)
    {
        Dictionary<IZone, ReadOnlyBitSet8> zones = new();
        var neighbors = new[]
        {
            new InfiniteBitmap(start.RowCount, start.ColumnCount),
            new InfiniteBitmap(start.RowCount, start.ColumnCount),
            new InfiniteBitmap(start.RowCount, start.ColumnCount),
            new InfiniteBitmap(start.RowCount, start.ColumnCount),
            new InfiniteBitmap(start.RowCount, start.ColumnCount)
        };

        for (int row = 0; row < start.RowCount; row++)
        {
            for (int col = 0; col < start.ColumnCount; col++)
            {
                var zone = start.GetZone(row, col);
                var bitSet = zones.TryGetValue(zone, out var bs) ? bs : new ReadOnlyBitSet8();
                var number = start[row, col];
                if (number != 0)
                {
                    neighbors[number - 1].Add(row, col);
                    bitSet += number;
                }
                
                zones[zone] = bitSet;
            }
        }

        Search(result, start, giver, zones, neighbors, 0, stopAt);
    }

    private static bool Search(IBackTrackingResult<ITectonic> result, ITectonic current, IPossibilitiesGiver giver,
        IDictionary<IZone, ReadOnlyBitSet8> zones, IReadOnlyList<InfiniteBitmap> neighbors, int position, int stopAt)
    {
        var full = current.RowCount * current.ColumnCount;
        for (; position < full; position++)
        {
            var row = position / current.ColumnCount;
            var col = position % current.ColumnCount;
            
            if (current[row, col] != 0) continue;

            var zone = current.GetZone(row, col);
            var bitSet = zones.TryGetValue(zone, out var bs) ? bs : new ReadOnlyBitSet8();
            foreach (var possibility in giver.EnumeratePossibilitiesAt(row, col))
            {
                var n = neighbors[possibility - 1];
                if(bitSet.Contains(possibility) || n.HasNeighbor(row, col)) continue;

                current[row, col] = possibility;
                //TODO non readonly bit set ?
                zones[zone] = bitSet + possibility;
                n.Add(row, col);

                if (Search(result, current, giver, zones, neighbors, position + 1, stopAt))
                {
                    current[row, col] = 0;
                    return true;
                }
                
                current[row, col] = 0;
                zones[zone] = bitSet;
                n.Remove(row, col);
            }

            return false;
        }

        result.AddNewResult(current);
        return result.Count >= stopAt;
    }
    
    private static void Start(IBackTrackingResult<Sudoku> result, Sudoku start, IPossibilitiesGiver giver, int stopAt)
    {
        var positions = new GridPositions[]
        {
            new(), new(), new(), new(), new(), new(), new(), new(), new()
        };

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var n = start[row, col];
                if (n == 0) continue;

                positions[n - 1].Add(row, col);
            }
        }
        
        Search(result, start, giver, positions, 0, stopAt);
    }


    private static bool Search(IBackTrackingResult<Sudoku> result, Sudoku current, IPossibilitiesGiver giver,
        IReadOnlyList<GridPositions> positions, int position, int stopAt)
    {
        for (; position < 81; position++)
        {
            var row = position / 9;
            var col = position % 9;

            if (current[row, col] != 0) continue;
            
            foreach (var possibility in giver.EnumeratePossibilitiesAt(row, col))
            {
                var pos = positions[possibility - 1];
                if (pos.IsRowNotEmpty(row) || pos.IsColumnNotEmpty(col) || pos.IsMiniGridNotEmpty(row / 3, col / 3)) continue;

                current[row, col] = possibility;
                pos.Add(row, col);

                if (Search(result, current, giver, positions, position + 1, stopAt))
                {
                    current[row, col] = 0;
                    return true;
                }

                current[row, col] = 0;
                pos.Remove(row, col);
            }

            return false;
        }
        
        result.AddNewResult(current.Copy());
        return result.Count >= stopAt;
    }
}

public interface IPossibilitiesGiver
{
    IEnumerable<int> EnumeratePossibilitiesAt(int row, int col);
}

public class TectonicPossibilitiesGiver : IPossibilitiesGiver
{
    private readonly ITectonic _tectonic;

    public TectonicPossibilitiesGiver(ITectonic tectonic)
    {
        _tectonic = tectonic;
    }

    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        var count = _tectonic.GetZone(row, col).Count;
        for (int i = 1; i <= count; i++)
        {
            yield return i;
        }
    }
}

public interface IBackTrackingResult<in T> where T : ICopyable<T>
{
    void AddNewResult(T sudoku);
    int Count { get; }
}

public class BackTrackingListResult<T> : List<T>, IBackTrackingResult<T> where T : ICopyable<T>
{
    public void AddNewResult(T sudoku)
    {
        Add(sudoku.Copy());
    }
}

public class BackTrackingCountResult<T> : IBackTrackingResult<T> where T : ICopyable<T>
{
    public void AddNewResult(T sudoku)
    {
        Count++;
    }

    public int Count { get; private set; }
}

public interface ICopyable<out T>
{
    T Copy();
}

