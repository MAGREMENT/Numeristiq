using Model.Sudoku;
using Model.Sudoku.Generator;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Tests;

public class BackTrackingTests
{
    [Test]
    public void MultipleSolutionsTest()
    {
        var sudoku = SudokuTranslator.TranslateLineFormat(
                ".9.2.......1..693..3.71..8.35........48..2.6...7......56....4......4.2..2...3.6.7");

        var result = BackTracking.Fill(sudoku, ConstantPossibilitiesGiver.Instance, int.MaxValue);
        
        Assert.That(result, Has.Length.EqualTo(26));
    }

}