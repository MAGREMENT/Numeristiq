using System.Collections.Generic;

namespace Model.Solver.StrategiesUtility;

public static class CombinationCalculator
{
    public static List<T[]> EveryCombination<T>(int count, IReadOnlyList<T> sample)
    {
        var result = new List<T[]>();

        EveryCombination(count, 0, sample, result, new List<T>());

        return result;
    }

    private static void EveryCombination<T>(int count, int start, IReadOnlyList<T> sample, List<T[]> result,
        List<T> current)
    {
        for (int i = start; i < sample.Count; i++)
        {
            var n = sample[i];
            current.Add(n);
            
            if(current.Count == count) result.Add(current.ToArray());
            else if (current.Count < count) EveryCombination(count, i + 1, sample, result, current);

            current.RemoveAt(current.Count - 1);
        }
    }
}