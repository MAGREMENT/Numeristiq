using Model.Core.Trackers;
using Model.Sudokus;
using Model.Sudokus.Generator;
using Model.Sudokus.Solver;
using Model.Utility;
using Repository;

namespace Tests.Sudokus;

public class SudokuPuzzleGeneratorTests
{
    private const int SudokuCount = 25;
    
    private readonly RDRSudokuPuzzleGenerator generator = new(new BackTrackingFilledSudokuGenerator());

    [Test]
    public void GenerationTest()
    {
        var instantiator = new PathInstantiator(true, true);
        var repo = new SudokuStrategiesJSONRepository(instantiator.Instantiate("strategies.json"));

        var solver = new SudokuSolver();
        solver.StrategyManager.AddStrategies(repo.Download());

        var finder = new HardestStrategyTracker<SudokuStrategy, ISudokuSolveResult>();
        finder.AttachTo(solver);
        
        var puzzles = generator.Generate(SudokuCount);
        
        Assert.Multiple(() =>
        {
            foreach (var p in puzzles)
            {
                Console.Write(SudokuTranslator.TranslateLineFormat(p, SudokuLineFormatEmptyCellRepresentation.Points));
                solver.SetSudoku(p.Copy());
                solver.Solve();
                Console.WriteLine(" - " + finder.Hardest?.Name);

                var solution = BackTracking.Solutions(p, ConstantPossibilitiesGiver.Instance, 2);
                Assert.That(solution, Has.Count.EqualTo(1));
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

                var solution = BackTracking.Solutions(p, ConstantPossibilitiesGiver.Instance, 2);
                Assert.That(solution, Has.Count.EqualTo(1));
                Assert.That(solution[0].IsCorrect, Is.True);
            }
        });
    }
}