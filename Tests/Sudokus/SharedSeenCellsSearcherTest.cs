using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility.SharedSeenCellSearchers;
using Tests.Utility;

namespace Tests.Sudokus;

public class SharedSeenCellsSearcherTest
{
    [Test]
    public void Test()
    {
        var expected = new GridPositions[80, 81];
        var defaultSearcher = new FullGridCheckSearcher();
        for (int first = 0; first < 80; first++)
        {
            for (int second = first + 1; second < 81; second++)
            {
                expected[first, second] = new GridPositions(defaultSearcher.SharedSeenCells(
                    first / 9, first % 9, second / 9, second % 9));
            }
        }

        Assert.Multiple(() => ImplementationSpeedComparator.Compare<ISharedSeenCellSearcher>(value =>
        {
            for (int first = 0; first < 80; first++)
            {
                for (int second = first + 1; second < 81; second++)
                {
                    var gp = expected[first, second];
                    var total = 0;
                    foreach (var cell in value.SharedSeenCells(first / 9, first % 9, second / 9, second % 9))
                    {
                        Assert.That(gp.Contains(cell), Is.True);
                        total++;
                    }
                    
                    Assert.That(gp, Has.Count.EqualTo(total));
                }
            }
        }, 50, new FullGridCheckSearcher(), new GridPositionsSearcher(),
            new InCommonFindSearcher(), new SeenCellCompareSearcher(), new BufferedSearcher(new InCommonFindSearcher())));
    }
}