using Model.Core.BackTracking;
using Model.Core.Trackers;
using Model.Sudokus;
using Model.Sudokus.Generator;
using Model.Sudokus.Solver;
using Repository;

namespace Tests.Sudokus;

public class SudokuPuzzleGeneratorTests
{
    private const int SudokuCount = 25;
    
    private readonly RDRSudokuPuzzleGenerator generator = new(new BackTrackingFilledSudokuGenerator());

    [Test]
    public void GenerationTest()
    {
        var repo = new SudokuStrategyJsonRepository("strategies.json", true, true);

        var solver = new SudokuSolver();
        solver.StrategyManager.AddStrategies(repo.GetStrategies());

        var finder = new HardestStrategyTracker();
        finder.AttachTo(solver);
        
        var puzzles = generator.Generate(SudokuCount);
        var backTracker = new SudokuBackTracker(new Sudoku(), ConstantPossibilitiesGiver.Nine)
        {
            StopAt = 2
        };
        
        Assert.Multiple(() =>
        {
            foreach (var p in puzzles)
            {
                Console.Write(SudokuTranslator.TranslateLineFormat(p, SudokuLineFormatEmptyCellRepresentation.Points));
                solver.SetSudoku(p.Copy());
                solver.Solve();
                Console.WriteLine(" - " + finder.Hardest?.Name);

                backTracker.Set(p);
                var solution = backTracker.Solutions();
                Assert.That(solution, Has.Count.EqualTo(1));
                Assert.That(solution[0].IsCorrect(), Is.True);
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

                backTracker.Set(p);
                var solution = backTracker.Solutions();
                Assert.That(solution, Has.Count.EqualTo(1));
                Assert.That(solution[0].IsCorrect(), Is.True);
            }
        });
    }
}