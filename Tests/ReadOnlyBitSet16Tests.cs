using Model.Utility.BitSets;

namespace Tests;

public class ReadOnlyBitSet16Tests
{
    [Test]
    public void IsFilledProperly()
    {
        var set = ReadOnlyBitSet16.Filled(2, 4);
        
        Assert.Multiple(() =>
        {
            Assert.That(set, Has.Count.EqualTo(3));
            Assert.That(set.Contains(2), Is.True);
            Assert.That(set.Contains(3), Is.True);
            Assert.That(set.Contains(4), Is.True);
        });
    }
}