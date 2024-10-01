using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Tests.Utility;

namespace Tests.Sudokus;

public class RestrictedPossibilityAlgorithmsTests
{
    /*
        ALS count : 133
        #1 AreRestricted
           Total: 6,4386448 s
           Average: 321,93224 ms
           Minimum: 295,5487 ms on try #11
           Maximum: 412,5708 ms on try #19
           Ignored: 1

        #2 AreRestricted
           Total: 13,39854 s
           Average: 669,927 ms
           Minimum: 533,4868 ms on try #21
           Maximum: 791,1527 ms on try #10
           Ignored: 1

        #3 AreRestricted
           Total: 6,5743071 s
           Average: 328,715355 ms
           Minimum: 237,862 ms on try #8
           Maximum: 522,6523 ms on try #4
           Ignored: 1

        #4 AreRestricted
           Total: 6,3653595 s
           Average: 318,267975 ms
           Minimum: 235,4947 ms on try #14
           Maximum: 426,0083 ms on try #10
           Ignored: 1
     */
    [Test]
    public void Test()
    {
        var sudoku = SudokuTranslator
            .TranslateLineFormat("3s3s2s4s96  7s7s4 138  2 8   4s10s64  1  3   3   7  2s4s78 9s7s2");
        var solver = new SudokuSolver();
        solver.SetSudoku(sudoku);
        
        var als = AlmostNakedSetSearcher.FullGrid(solver, 5, 1);
        Console.WriteLine("ALS count : " + als.Count);

        var expected = new bool[als.Count - 1, als.Count, 9];
        for (int i = 0; i < als.Count - 1; i++)
        {
            for (int j = 0; j < als.Count; j++)
            {
                foreach (var p in als[i].EnumeratePossibilities())
                {
                    expected[i, j, p - 1] = RestrictedPossibilityAlgorithms.EachCaseSearch(als[i], als[j], p);
                }
            }
        }
        
        ImplementationSpeedComparator.Compare<AreRestricted>(impl =>
        {
            for (int i = 0; i < als.Count - 1; i++)
            {
                for (int j = 0; j < als.Count; j++)
                {
                    if (als[i] is not SnapshotPossibilitySet als1 
                        || als[j] is not SnapshotPossibilitySet als2) continue;
                    
                    foreach (var p in als1.EnumeratePossibilities())
                    {
                        var result = impl(als1, als2, p);
                        Assert.That(result, Is.EqualTo(expected[i, j, p - 1]));
                    }
                }
            }
        }, 20, RestrictedPossibilityAlgorithms.EachCaseSearch,
            RestrictedPossibilityAlgorithms.GridPositionsSearch,
            RestrictedPossibilityAlgorithms.CommonHouseSearch,
            RestrictedPossibilityAlgorithms.AlternatingCommonHouseSearch);
    }
}

public delegate bool AreRestricted(IPossibilitySet left, IPossibilitySet right, int p);