using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Utility.BitSets;
using Tests.Utility;

namespace Tests.Sudokus;

public class RestrictedPossibilityAlgorithmsTests
{
    [Test]
    public void Test()
    {
        var sudoku = SudokuTranslator
            .TranslateLineFormat("3s3s2s4s96  7s7s4 138  2 8   4s10s64  1  3   3   7  2s4s78 9s7s2");
        var solver = new SudokuSolver();
        solver.SetSudoku(sudoku);
        
        var als = solver.AlmostNakedSetSearcher.FullGrid(5, 1);
        Console.WriteLine("ALS count : " + als.Count);

        var expected = new bool[als.Count - 1, als.Count, 9];
        for (int i = 0; i < als.Count - 1; i++)
        {
            for (int j = 0; j < als.Count; j++)
            {
                foreach (var p in als[i].Possibilities.EnumeratePossibilities())
                {
                    expected[i, j, p - 1] = RestrictedPossibilityAlgorithms.ForeachSearch(als[i], als[j], p);
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
                    
                    foreach (var p in als1.Possibilities.EnumeratePossibilities())
                    {
                        var result = impl(als1, als2, p);
                        Assert.That(result, Is.EqualTo(expected[i, j, p - 1]));
                    }
                }
            }
        }, 20, RestrictedPossibilityAlgorithms.ForeachSearch,
            RestrictedPossibilityAlgorithms.GridPositionsSearch,
            RestrictedPossibilityAlgorithms.CellEnumerationSearch);
    }
}

public delegate bool AreRestricted(IPossibilitySet left, IPossibilitySet right, int p);