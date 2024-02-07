using Model.Sudoku;
using Model.Sudoku.Generator;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;
using Repository;

namespace Tests;

public class SudokuPuzzleGenerator
{
    private const int SudokuCount = 25;
    
    private readonly ISudokuPuzzleGenerator generator =
        new RCRSudokuPuzzleGenerator(new BackTrackingFilledSudokuGenerator());

    [Test]
    public void GenerationTest()
    {
        var repo = new JSONRepository<List<StrategyDAO>>("strategies.json");
        try
        {
            repo.Initialize();
        }
        catch (RepositoryInitializationException)
        {
            Assert.That(true, Is.False);
        }

        var solver = new SudokuSolver
        {
            ChangeManagement = ChangeManagement.Fast
        };
        solver.Bind(repo);

        var finder = new HardestStrategyFinder(solver);
        
        var puzzles = generator.Generate(SudokuCount);
        
        Assert.Multiple(() =>
        {
            foreach (var p in puzzles)
            {
                Console.Write(SudokuTranslator.TranslateLineFormat(p, SudokuTranslationType.Points));
                solver.SetSudoku(p);
                finder.Clear();
                solver.Solve();
                Console.WriteLine(" - " + finder.Hardest.StrategyName);

                var solution = BackTracking.Fill(p, ConstantPossibilitiesGiver.Instance, 2);
                Assert.That(solution, Has.Length.EqualTo(1));
                Assert.That(solution[0].IsCorrect, Is.True);
            }
        });
    }
}