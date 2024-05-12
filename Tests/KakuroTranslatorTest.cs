using Model.Kakuros;

namespace Tests;

public class KakuroTranslatorTest
{
    [Test]
    public void TranslateSumFormatTest()
    {
        const string s = "0,0v17,2;0,0>16,2;1,0>17,3;2,1>10,2;0,1v11,3;1,2v15,2";

        var kakuro = KakuroTranslator.TranslateSumFormat(s);
        
        Assert.Multiple(() =>
        {
            Assert.That(kakuro.RowCount, Is.EqualTo(3));
            Assert.That(kakuro.ColumnCount, Is.EqualTo(3));
            Assert.That(kakuro.Sums, Has.Count.EqualTo(6));
        });
    }
}