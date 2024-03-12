using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Highlighting;
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
        _solver.ChangeBuffer = new LogManagedChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(_solver);
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
            new SolverProgress(ProgressType.PossibilityRemoval, 8, 1, 2),
            new SolverProgress(ProgressType.PossibilityRemoval, 8, 2, 3));
    }

    [Test]
    public void XYWing1Test()
    {
        TestSudokuStrategyInstance(new XYWingStrategy(),
            "g188aa050hca5090b0cg118g21g1c0050903ea05aa8211ca4gg1agaig1bi41228g09059g8i8g05g10911218i41aq41bq8g2205g19i9g8g21g11i050i9i41091103880o41g18g2105050o411q81211i1gg1",
            new SolverProgress(ProgressType.PossibilityRemoval, 4, 6, 3),
            new SolverProgress(ProgressType.PossibilityRemoval, 4, 6, 5));
    }

    [Test]
    public void XYWing2Test()
    {
        TestSudokuStrategyInstance(new XYWingStrategy(),
            "2111k8co4oso03480505g2812248i20h48110a0h4a11054a81g121gi09210m810ig41141gg81h04s215og4030o4105120qg11q21810o11410hg10305092181g8i005a811a8410h0381220a6o4o6o1105g1",
            new SolverProgress(ProgressType.PossibilityRemoval, 6, 7, 3));
    }

    private void TestSudokuStrategyInstance(SudokuStrategy strategy, string stateBefore32, params SolverProgress[] expected)
    {
        strategy.OnCommitBehavior = OnCommitBehavior.WaitForAll;
        _solver.StrategyManager.AddStrategy(strategy);
        _solver.SetState(SudokuTranslator.TranslateBase32Format(stateBefore32, new AlphabeticalBase32Translator()));
        _solver.Solve(true);

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