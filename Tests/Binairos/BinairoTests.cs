using Model.Binairos;

namespace Tests.Binairos;

public class BinairoTests
{
    [Test]
    public void BitSetTest()
    {
        var b = BinairoTranslator.TranslateLineFormat("6x6:00....0......1......00...........1..");
        b[5, 0] = 2;
        b[5, 1] = 2;
        b[5, 2] = 1;
        b[5, 4] = 1;
        b[5, 5] = 1;

        var set = b.RowSetAt(5);
        Assert.That(set.OnesCount, Is.EqualTo(3));
        Assert.That(set.TwosCount, Is.EqualTo(3));
    }
    
    [Test]
    public void CopyTest()
    {
        var b = BinairoTranslator.TranslateLineFormat("6x6:1...1..1........0...0.0......1.0.0..");
        var copy = b.Copy();
        
        Assert.That(b, Is.EqualTo(copy));
    }
}