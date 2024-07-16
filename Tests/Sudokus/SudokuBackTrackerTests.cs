using Model.Core.BackTracking;
using Model.Sudokus;

namespace Tests.Sudokus;

public class SudokuBackTrackerTests
{
    [Test]
    public void MultipleSolutionsTest()
    {
        var sudoku = SudokuTranslator.TranslateLineFormat(
                ".9.2.......1..693..3.71..8.35........48..2.6...7......56....4......4.2..2...3.6.7");
        var copy = sudoku.Copy();
        var backTracker = new SudokuBackTracker(sudoku, ConstantPossibilitiesGiver.Instance);

        var result = backTracker.Solutions();
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(26));
            Assert.That(sudoku, Is.EqualTo(copy));
        });

        backTracker.StopAt = 2;
        backTracker.Set(sudoku);
        result = backTracker.Solutions();
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(sudoku, Is.EqualTo(copy));
        });
    }
}