using Model.Nonograms;

namespace Tests.Nonograms;

public class NonogramTranslatorTests
{
    [Test]
    public void TestWithoutCells()
    {
        const string s = "4-3-2.1-1-2::1.1-1.3-2-3-2";

        var nonogram = NonogramTranslator.TranslateLineFormat(s);
        Console.WriteLine("Result :\n");
        Console.WriteLine(nonogram);
        var back = NonogramTranslator.TranslateLineFormat(nonogram);
        
        Assert.That(back, Is.EqualTo(s));
    }

    [Test]
    public void TestWithCells()
    {
        const string s = "4-3-2.1-1-2::1.1-1.3-2-3-2::0.0-2.2-3.1";

        var nonogram = NonogramTranslator.TranslateLineFormat(s);
        Console.WriteLine("Result :\n");
        Console.WriteLine(nonogram);
        var back = NonogramTranslator.TranslateLineFormat(nonogram);
        
        Assert.That(back, Is.EqualTo(s));
    }
}