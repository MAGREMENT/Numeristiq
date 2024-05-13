using System.Collections.Generic;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public class RecursiveKakuroCombinationCalculator : IKakuroCombinationCalculator
{
    public IEnumerable<IReadOnlyList<int>> CalculateCombinations(int amount, int cellCount)
    {
        if (cellCount <= 1) return new[] {new [] {amount}};
        
        List<IReadOnlyList<int>> result = new();
        Calculate(result, new List<int>(), 0, amount, cellCount, 1);
        return result;
    }

    public ReadOnlyBitSet16 CalculatePossibilities(int amount, int cellCount)
    {
        if (cellCount <= 1) return new ReadOnlyBitSet16(amount);
        
        ReadOnlyBitSet16 result = new();
        Calculate(ref result, new List<int>(), 0, amount, cellCount, 1);
        return result;
    }

    public ReadOnlyBitSet16 CalculatePossibilities(int amount, int cellCount, List<int> forced)
    {
        ReadOnlyBitSet16 result = new();
        CalculateWithForced(ref result, new List<int>(), 0, amount, cellCount, 1);
        return result;
    }

    private static void Calculate(List<IReadOnlyList<int>> result, List<int> currentNumbers, int currentTotal,
        int amount, int cellCount, int start)
    {
        for (; start < 9; start++)
        {
            currentNumbers.Add(start);
            var newTotal = currentTotal + start;
                
            if (currentNumbers.Count == cellCount - 1)
            {
                var last = amount - newTotal;
                if (last >= start + 1 && last is >= 1 and <= 9)
                {
                    currentNumbers.Add(last);
                    result.Add(currentNumbers.ToArray());
                    currentNumbers.RemoveAt(currentNumbers.Count - 1);
                }
            }
            else Calculate(result, currentNumbers, newTotal, amount, cellCount, start + 1);
            
            currentNumbers.RemoveAt(currentNumbers.Count - 1);
        }
    }
    
    private static void CalculateWithForced(ref ReadOnlyBitSet16 result, List<int> currentNumbers, int currentTotal,
        int amount, int cellCount, int start)
    {
        for (; start < 9; start++)
        {
            if (currentNumbers.Contains(start)) continue;
            
            currentNumbers.Add(start);
            var newTotal = currentTotal + start;
                
            if (currentNumbers.Count == cellCount - 1)
            {
                var last = amount - newTotal;
                if (last >= start + 1 && last is >= 1 and <= 9 && !currentNumbers.Contains(last))
                {
                    foreach (var p in currentNumbers)
                    {
                        result += p;
                    }

                    result += last;
                }
            }
            else Calculate(ref result, currentNumbers, newTotal, amount, cellCount, start + 1);
            
            currentNumbers.RemoveAt(currentNumbers.Count - 1);
        }
    } 

    private static void Calculate(ref ReadOnlyBitSet16 result, List<int> currentNumbers, int currentTotal,
        int amount, int cellCount, int start)
    {
        for (; start < 9; start++)
        {
            currentNumbers.Add(start);
            var newTotal = currentTotal + start;
                
            if (currentNumbers.Count == cellCount - 1)
            {
                var last = amount - newTotal;
                if (last >= start + 1 && last is >= 1 and <= 9)
                {
                    foreach (var p in currentNumbers)
                    {
                        result += p;
                    }

                    result += last;
                }
            }
            else Calculate(ref result, currentNumbers, newTotal, amount, cellCount, start + 1);
            
            currentNumbers.RemoveAt(currentNumbers.Count - 1);
        }
    } 
}