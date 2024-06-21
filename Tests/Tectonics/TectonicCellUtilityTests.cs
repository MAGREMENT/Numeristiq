using Model.Tectonics;
using Model.Utility;

namespace Tests.Tectonics;

public class TectonicCellUtilityTests
{
    [Test]
    public void NonAdjacentTest()
    {
        Assert.That(TectonicCellUtility.AreAdjacent(new Cell(1, 0), new Cell(0, 2)), Is.False);
    }
}