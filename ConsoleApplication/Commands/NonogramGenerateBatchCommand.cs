using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Core.Generators;
using Model.Core.Trackers;
using Model.Nonograms;
using Model.Nonograms.Generator;
using Model.Nonograms.Solver;
using Model.Utility;

namespace ConsoleApplication.Commands;

public class NonogramGenerateBatchCommand : GenerateBatchWithRandomSizeCommand<Nonogram>
{
    public NonogramGenerateBatchCommand() : base("Nonogram", new RandomNonogramGenerator())
    {
    }

    protected override (ISolver, IRatingTracker, IHardestStrategyTracker) GetSolverWithAttachedTracker(ArgumentInterpreter interpreter)
    {
        var solver = interpreter.Instantiator.InstantiateNonogramSolver();
        var ratings = new RatingTracker();
        var hardest = new HardestStrategyTracker();

        ratings.AttachTo(solver);
        hardest.AttachTo(solver);

        return (solver, ratings, hardest);
    }

    protected override GeneratedPuzzle<Nonogram> CreateGeneratedPuzzle(Nonogram puzzle)
    {
        return new GeneratedNonogramPuzzle(puzzle);
    }

    protected override void SetPuzzle(ISolver solver, Nonogram puzzle)
    {
        ((NonogramSolver)solver).SetNonogram(puzzle);
    }

    protected override GridSizeRandomizer GetRandomizer(IPuzzleGenerator<Nonogram> generator)
    {
        return ((RandomNonogramGenerator)generator).Randomizer;
    }
}