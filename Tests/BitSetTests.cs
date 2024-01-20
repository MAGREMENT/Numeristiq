using Model.Utility;

namespace Tests;

public class BitSetTests
{
    [Test]
    public void InsertTest()
    {
        var bs = new InfiniteBitSet();
        bs.Set(2);
        bs.Set(0);
        bs.Insert(2);

        Assert.True(bs.IsSet(0));
        Assert.False(bs.IsSet(1));
        Assert.False(bs.IsSet(2));
        Assert.True(bs.IsSet(3));
        Assert.False(bs.IsSet(4));
    }
    
    [Test]
    public void DeleteTest()
    {
        var bs = new InfiniteBitSet();
        bs.Set(2);
        bs.Set(4);
        bs.Set(0);
        bs.Delete(2);

        Assert.True(bs.IsSet(0));
        Assert.False(bs.IsSet(1));
        Assert.False(bs.IsSet(2));
        Assert.True(bs.IsSet(3));
        Assert.False(bs.IsSet(4));
    }
}