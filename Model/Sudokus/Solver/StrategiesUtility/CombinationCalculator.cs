using System.Collections.Generic;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.StrategiesUtility;

public static class CombinationCalculator
{
    public static readonly int[] NumbersSample = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    public static List<ReadOnlyBitSet16> EveryPossibilityCombinationWithSpecificCount(int count)
    {
        var result = new List<ReadOnlyBitSet16>();

        EveryPossibilityCombinationWithSpecificCount(count, 0, result, new ReadOnlyBitSet16());

        return result;
    }

    private static void EveryPossibilityCombinationWithSpecificCount(int count, int start,
        List<ReadOnlyBitSet16> result, ReadOnlyBitSet16 current)
    {
        for (int i = start; i < NumbersSample.Length; i++)
        {
            var next = current + NumbersSample[i];
            
            if(next.Count == count) result.Add(next);
            else if (next.Count < count) EveryPossibilityCombinationWithSpecificCount(count, i + 1, result, next);
        }
    }
    
    public static List<T[]> EveryCombinationWithSpecificCount<T>(int count, IReadOnlyList<T> sample)
    {
        var result = new List<T[]>();

        EveryCombinationWithSpecificCount(count, 0, sample, result, new List<T>());

        return result;
    }

    private static void EveryCombinationWithSpecificCount<T>(int count, int start, IReadOnlyList<T> sample, List<T[]> result,
        List<T> current)
    {
        for (int i = start; i < sample.Count; i++)
        {
            current.Add(sample[i]);
            
            if(current.Count == count) result.Add(current.ToArray());
            else if (current.Count < count) EveryCombinationWithSpecificCount(count, i + 1, sample, result, current);

            current.RemoveAt(current.Count - 1);
        }
    }
    
    public static List<T[]> EveryCombinationWithMaxCount<T>(int max, IReadOnlyList<T> sample)
    {
        var result = new List<T[]>();

        EveryCombinationWithMaxCount(max, 0, sample, result, new List<T>());

        return result;
    }

    private static void EveryCombinationWithMaxCount<T>(int max, int start, IReadOnlyList<T> sample, List<T[]> result,
        List<T> current)
    {
        for (int i = start; i < sample.Count; i++)
        {
            current.Add(sample[i]);
            
            result.Add(current.ToArray()); 
            if (current.Count < max) EveryCombinationWithMaxCount(max, i + 1, sample, result, current);

            current.RemoveAt(current.Count - 1);
        }
    }
}