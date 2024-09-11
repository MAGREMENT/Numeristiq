using Model.Utility;

namespace Tests;

public class CellUtilityTests
{
    [Test]
    public void AreAllAdjacentTest()
    {
        List<Cell> list = new()
        {
            new Cell(0,1), new Cell(1,1), new Cell(2,1), new Cell(1,0)
        };
        
        Assert.That(CellUtility.AreAllAdjacent(list), Is.True);
    }
}