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

    [Test]
    public void SharedSeenCellsTest()
    {
        var t = TectonicTranslator.TranslateRdFormat("5.4:1d0r0r2d0r0rd000d00d05rd0d0rd3d04");
        TestSharedSeenCellsForTectonic(t);
    }

    private static void TestSharedSeenCellsForTectonic(IReadOnlyTectonic tectonic)
    {
        var max = tectonic.RowCount * tectonic.ColumnCount;
        for (int i = 0; i < max - 1; i++)
        {
            for (int j = i + 1; j < max; j++)
            {
                var c1 = new Cell(i / tectonic.RowCount, i % tectonic.ColumnCount);
                var c2 = new Cell(j / tectonic.RowCount, j % tectonic.ColumnCount);

                var naive = new List<Cell>(NaiveSharedSeenCells(tectonic, c1, c2));
                var utility = new List<Cell>(TectonicCellUtility.SharedSeenCells(tectonic, c1, c2));

                Assert.That(naive, Has.Count.EqualTo(utility.Count));
                foreach (var c in naive)
                {
                    Assert.That(utility, Does.Contain(c));
                }
            }
        }
    }

    private static IEnumerable<Cell> NaiveSharedSeenCells(IReadOnlyTectonic tectonic, Cell one, Cell two)
    {
        for (int row = 0; row < tectonic.RowCount; row++)
        {
            for (int col = 0; col < tectonic.ColumnCount; col++)
            {
                var current = new Cell(row, col);
                if (current == one || current == two) continue;
                
                if (TectonicCellUtility.DoesSeeEachOther(tectonic, current, one) &&
                    TectonicCellUtility.DoesSeeEachOther(tectonic, current, two)) yield return current;
            }
        }
    }
}