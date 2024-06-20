using Model.Kakuros;
using Model.Utility;

namespace Tests.Kakuro;

public class IKakuroTest
{
    [Test]
    public void Test()
    {
        TestInstance(new SumListKakuro());
    }
    
    private void TestInstance(IKakuro kakuro)
    {
        Assert.Multiple(() =>
        {
            Assert.That(kakuro.RowCount, Is.EqualTo(0));
            Assert.That(kakuro.ColumnCount, Is.EqualTo(0));
            Assert.That(kakuro.Sums, Is.Empty);
        });

        kakuro.AddSum(new VerticalKakuroSum(new Cell(0, 0), 10, 3));
        FirstInstanceTest(kakuro);

        kakuro.AddSum(new HorizontalKakuroSum(new Cell(1, 0), 17, 4));
        FirstInstanceTest(kakuro);

        kakuro.AddSum(new HorizontalKakuroSum(new Cell(1, 0), 17, 1));
        var sum = kakuro.HorizontalSumFor(new Cell(1, 0));
        Assert.That(sum, Is.Not.Null);
        Assert.That(sum!.Amount, Is.EqualTo(17));

        kakuro.AddSum(new VerticalKakuroSum(new Cell(0, 1), 17, 4));
        Assert.Multiple(() =>
        {
            Assert.That(kakuro.RowCount, Is.EqualTo(4));
            Assert.That(kakuro.ColumnCount, Is.EqualTo(2));
            Assert.That(kakuro.Sums.Count(), Is.EqualTo(6));
        });
        
        kakuro.ForceSum(new HorizontalKakuroSum(new Cell(0, 0), 10, 3));
        Assert.Multiple(() =>
        {
            Assert.That(kakuro.RowCount, Is.EqualTo(4));
            Assert.That(kakuro.ColumnCount, Is.EqualTo(3));
            Assert.That(kakuro.Sums.Count(), Is.EqualTo(7));
        });
        
        kakuro.ForceSum(new HorizontalKakuroSum(new Cell(1, 0), 10, 3));
        Assert.Multiple(() =>
        {
            Assert.That(kakuro.RowCount, Is.EqualTo(4));
            Assert.That(kakuro.ColumnCount, Is.EqualTo(3));
            Assert.That(kakuro.Sums.Count(), Is.EqualTo(7));

            var hs = kakuro.HorizontalSumFor(new Cell(0, 0));
            Assert.That(hs, Is.Not.Null);
            Assert.That(hs, Is.EqualTo(new HorizontalKakuroSum(new Cell(0, 0), 10, 3)));
        });
    }

    private void FirstInstanceTest(IKakuro kakuro)
    {
        Assert.Multiple(() =>
        {
            Assert.That(kakuro.RowCount, Is.EqualTo(3));
            Assert.That(kakuro.ColumnCount, Is.EqualTo(1));
            Assert.That(kakuro.Sums.Count(), Is.EqualTo(4));
            int count = 0;
            foreach (var cell in kakuro.EnumerateCells())
            {
                count++;
                Assert.That(kakuro.HorizontalSumFor(cell), Is.Not.Null);
                Assert.That(kakuro.VerticalSumFor(cell), Is.Not.Null);
            }
            
            Assert.That(count, Is.EqualTo(3));
        });
    }
}