using System.Collections.Generic;

namespace Model.Sudoku.Solver.StrategiesUtility;

public static class CombinationCalculator
{
    public static readonly int[] NumbersSample = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    
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
            var n = sample[i];
            current.Add(n);
            
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
            var n = sample[i];
            current.Add(n);
            
            result.Add(current.ToArray()); 
            if (current.Count < max) EveryCombinationWithMaxCount(max, i + 1, sample, result, current);

            current.RemoveAt(current.Count - 1);
        }
    }
}