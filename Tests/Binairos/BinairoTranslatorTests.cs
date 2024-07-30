using Model.Binairos;

namespace Tests.Binairos;

public class BinairoTranslatorTests
{
    [Test]
    public void Test()
    {
        const string s = "3x4:...1.01.11..";

        var binairo = BinairoTranslator.TranslateLineFormat(s);
        Console.WriteLine(binairo);

        Assert.That(BinairoTranslator.TranslateLineFormat(binairo), Is.EqualTo(s));
    }
}