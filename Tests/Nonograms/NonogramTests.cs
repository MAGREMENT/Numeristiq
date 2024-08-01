using Model.Nonograms;

namespace Tests.Nonograms;

public class NonogramTests
{
    [Test]
    public void CorrectnessTest()
    {
        var n = NonogramTranslator.TranslateLineFormat("4-3-2.1-1-2::1.1-1.3-2-3-2");
        var copy = n.Copy();
        n[4, 1] = true;
        n[4, 4] = true;
        n[0, 2] = true;
        n[1, 2] = true;
        n[3, 2] = true;

        Assert.That(n.IsRowCorrect(4), Is.False);
        Assert.That(n.IsColumnCorrect(2), Is.True);

        copy[1, 2] = true;
        copy[3, 2] = true;
        copy[4, 2] = true;

        Assert.That(copy.IsColumnCorrect(2), Is.False);
    }
}