using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Strategies;

namespace Tests;

public class SudokuStrategiesTests
{
    private readonly SudokuSolver _solver = new();

    private readonly SudokuStrategyTestCase[] _testCases =
    {
        new(new SimpleColoringStrategy(), 
            "05k088880h03k011210hk81121c005s00348c0032148g111c40c0h0911c00305g1210hc0030h05c0214811g188c021g111880h440c0311810h0503210941g1g105480h4881032111214803g111480h8105",
            new SolverProgress(ProgressType.SolutionAddition, 7, 1, 4),
            new SolverProgress(ProgressType.SolutionAddition, 7, 4, 3),
            new SolverProgress(ProgressType.SolutionAddition, 7, 3, 8),
            new SolverProgress(ProgressType.SolutionAddition, 7, 5, 0),
            new SolverProgress(ProgressType.SolutionAddition, 7, 7, 2),
            new SolverProgress(ProgressType.SolutionAddition, 7, 8, 5))
    };

    [OneTimeSetUp]
    public void FirstSetUp()
    {
        _solver.ChangeBuffer = new LogManagedChangeBuffer(_solver);
    }

    [Test]
    public void TestCases()
    {
        foreach (var testCase in _testCases)
        {
            testCase.Test(_solver);
        }
    }
}

public class SudokuStrategyTestCase
{
    private readonly SudokuStrategy _strategy;
    private readonly string _stateBefore32;
    private readonly SolverProgress[] _expected;

    public SudokuStrategyTestCase(SudokuStrategy strategy, string stateBefore32, params SolverProgress[] expected)
    {
        _strategy = strategy;
        _stateBefore32 = stateBefore32;
        _expected = expected;
    }

    public void Test(SudokuSolver solver)
    {
        solver.StrategyManager.AddStrategy(_strategy);
        solver.SetState(SudokuTranslator.TranslateBase32Format(_stateBefore32, new AlphabeticalBase32Translator()));
        solver.Solve();

        List<SolverProgress> progresses = new();
        foreach (var log in solver.Logs)
        {
            progresses.AddRange(log.Changes);
        }
        
        Assert.Multiple(() =>
        {
            foreach (var progress in _expected)
            {
                Assert.That(progresses, Does.Contain(progress));
            }
        });
        
        solver.StrategyManager.ClearStrategies();
    }
}