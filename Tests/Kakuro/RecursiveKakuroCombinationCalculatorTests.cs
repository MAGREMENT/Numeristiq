using Model.Kakuros;
using Model.Utility.BitSets;

namespace Tests.Kakuro;

public class RecursiveKakuroCombinationCalculatorTests
{
    [Test]
    public void CombinationsTests()
    {
        var cc = new RecursiveKakuroCombinationCalculator();
        var result = new ReadOnlyBitSet16(7, 9);
        Assert.That(cc.CalculatePossibilities(16, 2), Is.EqualTo(result));
    }
}