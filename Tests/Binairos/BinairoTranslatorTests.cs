using Model.Binairos;

namespace Tests.Binairos;

public class BinairoTranslatorTests
{
    [Test]
    public void Test()
    {
        const string s = "6x6:1...1..1........0...0.0......1.0.0..";

        var binairo = BinairoTranslator.TranslateLineFormat(s);
        Console.WriteLine(binairo);

        Assert.That(BinairoTranslator.TranslateLineFormat(binairo), Is.EqualTo(s));
    }
}