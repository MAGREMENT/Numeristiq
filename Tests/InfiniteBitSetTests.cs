using Model.Utility.BitSets;

namespace Tests;

public class InfiniteBitSetTests
{
    [Test]
    public void ShiftLeftTest()
    {
        var bs = new InfiniteBitSet();
        bs.Set(2);
        bs.Set(0);
        bs.ShiftLeft(2);
        
        Assert.Multiple(() =>
        {
            Assert.That(bs.IsSet(0), Is.True);
            Assert.That(bs.IsSet(1), Is.False);
            Assert.That(bs.IsSet(2), Is.False);
            Assert.That(bs.IsSet(3), Is.True);
            Assert.That(bs.IsSet(4), Is.False);
        });
    }

    [Test]
    public void ShiftRightTest()
    {
        var bs = new InfiniteBitSet();
        bs.Set(2);
        bs.Set(4);
        bs.Set(0);
        bs.ShiftRight(2);
        
        Assert.Multiple(() =>
        {
            Assert.That(bs.IsSet(0), Is.True);
            Assert.That(bs.IsSet(1), Is.False);
            Assert.That(bs.IsSet(2), Is.False);
            Assert.That(bs.IsSet(3), Is.True);
            Assert.That(bs.IsSet(4), Is.False);
        });
    }
}