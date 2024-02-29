using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Strategies;

namespace Tests;

public class SudokuStrategiesTests
{
    private readonly SudokuSolver _solver = new();

    [OneTimeSetUp]
    public void FirstSetUp()
    {
        _solver.ChangeBuffer = new LogManagedChangeBuffer(_solver);
    }

    [TearDown]
    public void TearDown()
    {
        _solver.StrategyManager.ClearStrategies();
    }

    [Test]
    public void SimpleColoringTwiceInSameUnitTest()
    {
        TestSudokuStrategyInstance(new SimpleColoringStrategy(),
            "05k088880h03k011210hk81121c005s00348c0032148g111c40c0h0911c00305g1210hc0030h05c0214811g188c021g111880h440c0311810h0503210941g1g105480h4881032111214803g111480h8105",
            new SolverProgress(ProgressType.SolutionAddition, 7, 1, 4),
            new SolverProgress(ProgressType.SolutionAddition, 7, 4, 3),
            new SolverProgress(ProgressType.SolutionAddition, 7, 3, 8),
            new SolverProgress(ProgressType.SolutionAddition, 7, 5, 0),
            new SolverProgress(ProgressType.SolutionAddition, 7, 7, 2),
            new SolverProgress(ProgressType.SolutionAddition, 7, 8, 5));
    }

    [Test]
    public void SimpleColoringTwoColoursElsewhereTest()
    {
        TestSudokuStrategyInstance(new SimpleColoringStrategy(),
            "05l8d8c80h03s018210hl8d821d005s00348c00321c8g158c41c0h0950d00305g1210hc0030h05c8214811g1c8c021g111c80h440c0311810h0503210941g1g105480h4881032111214803g158580h8105",
            new SolverProgress(ProgressType.PossibilityRemoval, 8, 0, 6),
            new SolverProgress(ProgressType.PossibilityRemoval, 8, 1, 2),
            new SolverProgress(ProgressType.PossibilityRemoval, 8, 2, 3));
    }

    private void TestSudokuStrategyInstance(SudokuStrategy strategy, string stateBefore32, params SolverProgress[] expected)
    {
        strategy.OnCommitBehavior = OnCommitBehavior.WaitForAll;
        _solver.StrategyManager.AddStrategy(strategy);
        _solver.SetState(SudokuTranslator.TranslateBase32Format(stateBefore32, new AlphabeticalBase32Translator()));
        _solver.Solve();

        List<SolverProgress> progresses = new();
        foreach (var log in _solver.Logs)
        {
            progresses.AddRange(log.Changes);
        }
        
        Assert.Multiple(() =>
        {
            foreach (var progress in expected)
            {
                Assert.That(progresses, Does.Contain(progress));
            }
        });
    }
}