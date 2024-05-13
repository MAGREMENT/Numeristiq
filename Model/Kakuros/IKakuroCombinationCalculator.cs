using System.Collections.Generic;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public interface IKakuroCombinationCalculator
{
    IEnumerable<IReadOnlyList<int>> CalculateCombinations(int amount, int cellCount);
    ReadOnlyBitSet16 CalculatePossibilities(int amount, int cellCount);
    ReadOnlyBitSet16 CalculatePossibilities(int amount, int cellCount, List<int> forced);
}