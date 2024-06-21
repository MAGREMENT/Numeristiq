using Model.Kakuros;
using Model.Utility.BitSets;

namespace Tests.Kakuros;

public class RecursiveKakuroCombinationCalculatorTests
{
    [Test]
    public void PossibilitiesTests()
    {
        var cc = new RecursiveKakuroCombinationCalculator();
        var result = new ReadOnlyBitSet16(7, 9);
        Assert.That(cc.CalculatePossibilities(16, 2), Is.EqualTo(result));
    }
    
    [Test]
    public void CombinationsTests()
    {
        var cc = new RecursiveKakuroCombinationCalculator();
        foreach (var result in cc.CalculateCombinations(17, 3))
        {
            foreach (var n in result)
            {
                Console.Write($"{n} ");
            }
            Console.WriteLine();
        }
    }
}