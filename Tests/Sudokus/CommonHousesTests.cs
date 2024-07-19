using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Tests.Sudokus;

public class CommonHousesTests
{
    [Test]
    public void Test()
    {
        var ch = new CommonHouses();
        Assert.That(ch.IsValid(), Is.False);

        ch = new CommonHouses(new Cell(0, 0));
        Assert.That(ch.IsValid(), Is.True);

        ch = ch.Adapt(new Cell(1, 1));
        Assert.That(ch.IsValid(), Is.True);

        ch = ch.Adapt(new Cell(0, 8));
        Assert.That(ch.IsValid(), Is.False);
    }
}