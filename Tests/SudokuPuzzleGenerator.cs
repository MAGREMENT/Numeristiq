using Model.Sudoku;
using Model.Sudoku.Generator;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Tests;

public class SudokuPuzzleGenerator
{
    private const int SudokuCount = 5;

    private readonly ISudokuPuzzleGenerator _generator =
        new RCRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator());

    [Test]
    public void GenerationTest()
    {
        var puzzles = _generator.Generate(SudokuCount);
        
        Assert.Multiple(() =>
        {
            foreach (var p in puzzles)
            {
                Console.WriteLine(SudokuTranslator.TranslateLineFormat(p, SudokuTranslationType.Points));

                var solution = BackTracking.Fill(p, ConstantPossibilitiesGiver.Instance, 2);
                Assert.That(solution, Has.Length.EqualTo(1));
                Assert.That(solution[0].IsCorrect, Is.True);
            }
        });
    }
}