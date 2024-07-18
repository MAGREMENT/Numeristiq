using Model.Sudokus.Solver.Strategies;
using Model.Utility;

namespace Tests.Sudokus;

public class DistributedDisjointSubsetCoverTests
{
    [Test]
    public void Test()
    {
        var dds = new DistributedDisjointSubsetCover();
        Assert.That(dds.IsValid(), Is.False);

        dds = new DistributedDisjointSubsetCover(new Cell(0, 0));
        Assert.That(dds.IsValid(), Is.True);

        dds = dds.Adapt(new Cell(1, 1));
        Assert.That(dds.IsValid(), Is.True);

        dds = dds.Adapt(new Cell(0, 8));
        Assert.That(dds.IsValid(), Is.False);
    }
}