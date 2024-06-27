using Model.Utility.BitSets;

namespace Tests;

public class InfiniteBitSetTests
{
    [Test]
    public void ShiftLeftTest()
    {
        var bs = new InfiniteBitSet();
        bs.Add(2);
        bs.Add(0);
        bs.ShiftLeft(2);
        
        Assert.Multiple(() =>
        {
            Assert.That(bs.Contains(0), Is.True);
            Assert.That(bs.Contains(1), Is.False);
            Assert.That(bs.Contains(2), Is.False);
            Assert.That(bs.Contains(3), Is.True);
            Assert.That(bs.Contains(4), Is.False);
        });
    }

    [Test]
    public void ShiftRightTest()
    {
        var bs = new InfiniteBitSet();
        bs.Add(2);
        bs.Add(4);
        bs.Add(0);
        bs.ShiftRight(2);
        
        Assert.Multiple(() =>
        {
            Assert.That(bs.Contains(0), Is.True);
            Assert.That(bs.Contains(1), Is.False);
            Assert.That(bs.Contains(2), Is.False);
            Assert.That(bs.Contains(3), Is.True);
            Assert.That(bs.Contains(4), Is.False);
        });
    }
}