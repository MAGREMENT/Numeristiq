using Model.Sudokus;
using Model.Sudokus.Generator;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Trackers;
using Model.Sudokus.Solver.Utility;
using Repository;

namespace Tests;

public class SudokuPuzzleGenerator
{
    private const int SudokuCount = 25;
    
    private readonly RDRSudokuPuzzleGenerator generator = new(new BackTrackingFilledSudokuGenerator());

    [Test]
    public void GenerationTest()
    {
        var repo = new SudokuStrategiesJSONRepository("strategies.json");
        if (!repo.Initialize(false)) Assert.Fail();

        var solver = new SudokuSolver();
        solver.StrategyManager.AddStrategies(repo.Download());

        var finder = new HardestStrategyTracker();
        solver.AddTracker(finder);
        
        var puzzles = generator.Generate(SudokuCount);
        
        Assert.Multiple(() =>
        {
            foreach (var p in puzzles)
            {
                Console.Write(SudokuTranslator.TranslateLineFormat(p, SudokuLineFormatEmptyCellRepresentation.Points));
                solver.SetSudoku(p.Copy());
                solver.Solve();
                Console.WriteLine(" - " + finder.Hardest?.Name);

                var solution = BackTracking.Fill(p, ConstantPossibilitiesGiver.Instance, int.MaxValue);
                Assert.That(solution, Has.Length.EqualTo(1));
                Assert.That(solution[0].IsCorrect, Is.True);
            }
        });

        generator.KeepSymmetry = true;
        
        puzzles = generator.Generate(SudokuCount);
        
        Assert.Multiple(() =>
        {
            foreach (var p in puzzles)
            {
                Console.Write(SudokuTranslator.TranslateLineFormat(p, SudokuLineFormatEmptyCellRepresentation.Points));
                solver.SetSudoku(p.Copy());
                solver.Solve();
                Console.WriteLine(" - " + finder.Hardest?.Name);

                var solution = BackTracking.Fill(p, ConstantPossibilitiesGiver.Instance, int.MaxValue);
                Assert.That(solution, Has.Length.EqualTo(1));
                Assert.That(solution[0].IsCorrect, Is.True);
            }
        });
    }
}