using Model.Sudokus;
using Model.Sudokus.Generator;
using Model.Utility;

namespace Tests;

public class BackTrackingTests
{
    [Test]
    public void MultipleSolutionsTest()
    {
        var sudoku = SudokuTranslator.TranslateLineFormat(
                ".9.2.......1..693..3.71..8.35........48..2.6...7......56....4......4.2..2...3.6.7");
        var copy = sudoku.Copy();

        var result = BackTracking.Fill(sudoku, ConstantPossibilitiesGiver.Instance, int.MaxValue);
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(26));
            Assert.That(sudoku, Is.EqualTo(copy));
        });
        
        result = BackTracking.Fill(sudoku, ConstantPossibilitiesGiver.Instance, 2);
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(sudoku, Is.EqualTo(copy));
        });
    }
}