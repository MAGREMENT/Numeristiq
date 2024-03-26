using Model.Tectonic;
using Model.Utility;

namespace Tests;

public class TectonicCellUtilityTest
{
    [Test]
    public void NonAdjacentTest()
    {
        Assert.That(TectonicCellUtility.AreAdjacent(new Cell(1, 0), new Cell(0, 2)), Is.False);
    }
}