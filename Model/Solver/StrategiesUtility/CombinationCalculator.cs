using System.Collections.Generic;

namespace Model.Solver.StrategiesUtility;

public static class CombinationCalculator
{
    public static List<int[]> EveryCombination(int count, List<int> sample)
    {
        var result = new List<int[]>();

        EveryCombination(count, 0, sample, result, new List<int>());

        return result;
    }

    private static void EveryCombination(int count, int start, List<int> sample, List<int[]> result,
        List<int> current)
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