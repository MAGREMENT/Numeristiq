using Model.Nonograms;

namespace Tests.Nonograms;

public class NonogramTranslatorTests
{
    [Test]
    public void Test()
    {
        const string s = "4-3-2.1-1-2::1.1-1.3-2-3-2";

        var nonogram = NonogramTranslator.TranslateLineFormat(s);
        Console.WriteLine("Result :\n");
        Console.WriteLine(nonogram);
        var back = NonogramTranslator.TranslateLineFormat(nonogram);
        
        Assert.That(back, Is.EqualTo(s));
    }
}