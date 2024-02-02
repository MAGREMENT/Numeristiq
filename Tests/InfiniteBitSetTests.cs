using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Utility;

namespace Tests;

public class InfiniteBitSetTests
{
    [Test]
    public void InsertTest()
    {
        var bs = new InfiniteBitSet();
        bs.Set(2);
        bs.Set(0);
        bs.Insert(2);
        
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
    public void DeleteTest()
    {
        var bs = new InfiniteBitSet();
        bs.Set(2);
        bs.Set(4);
        bs.Set(0);
        bs.Delete(2);
        
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