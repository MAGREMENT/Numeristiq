using Model.Core.BackTracking;
using Model.Kakuros;

namespace Tests.Kakuros;

public class KakuroBackTrackerTest
{
    [Test]
    public void FillTest()
    {
        var kakuro = KakuroTranslator.TranslateSumFormat("0,0v12,2;0,0>17,2;1,0>6,3;2,1>4,2;0,1v12,3;1,2v3,2;");
        var bt = new KakuroBackTracker(kakuro, ConstantPossibilitiesGiver.Nine,
            new RecursiveKakuroCombinationCalculator());

        Assert.That(bt.Fill(), Is.True);
        Assert.That(kakuro.IsCorrect(), Is.True);

        kakuro = KakuroTranslator.TranslateSumFormat("0,0v23,3;0,1v7,3;0,2v21,3;0,0>18,3;1,0>20,3;2,0>13,3;");
        bt.Set(kakuro);

        var solutions = bt.Solutions();

        Assert.That(solutions, Has.Count.EqualTo(1));
        Assert.That(solutions[0].IsCorrect(), Is.True);
    }
}