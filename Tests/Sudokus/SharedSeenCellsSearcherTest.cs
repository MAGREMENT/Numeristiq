using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Tests.Utility;

namespace Tests.Sudokus;

public class SharedSeenCellsSearcherTest
{
    [Test]
    public void Test()
    {
        var expected = new GridPositions[80, 81];
        for (int first = 0; first < 80; first++)
        {
            for (int second = first + 1; second < 81; second++)
            {
                expected[first, second] = new GridPositions(SharedSeenAlgorithms.FullGridSharedSeenCells(
                    first / 9, first % 9, second / 9, second % 9));
            }
        }

        Assert.Multiple(() => ImplementationSpeedComparator.Compare<SharedSeenCells>(value =>
        {
            for (int first = 0; first < 80; first++)
            {
                for (int second = first + 1; second < 81; second++)
                {
                    var gp = expected[first, second];
                    var total = 0;
                    foreach (var cell in value(first / 9, first % 9, second / 9, second % 9))
                    {
                        Assert.That(gp.Contains(cell), Is.True);
                        total++;
                    }
                    
                    Assert.That(gp, Has.Count.EqualTo(total));
                }
            }
        }, 50, SharedSeenAlgorithms.FullGridSharedSeenCells, 
            SharedSeenAlgorithms.GridPositionsSharedSeenCells,
            SharedSeenAlgorithms.InCommonSharedSeenCells,
            SharedSeenAlgorithms.SharedUnitSharedSeenCells));
    }
}

public delegate IEnumerable<Cell> SharedSeenCells(int row1, int col1, int row2, int col2); 