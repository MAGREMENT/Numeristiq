using Model.CrossSums;

namespace Tests.CrossSum;

public class CrossSumTranslatorTests
{
    [Test]
    public void Test()
    {
        const string s = "14.14.10.10.3::3.5.27.6.10::1226837624698432914525793";

        var cs = CrossSumTranslator.Translate(s);
        Assert.That(CrossSumTranslator.Translate(cs), Is.EqualTo(s));
    }
}