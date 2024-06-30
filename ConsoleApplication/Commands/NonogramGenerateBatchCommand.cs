using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Core.Generators;
using Model.Core.Trackers;
using Model.Nonograms;
using Model.Nonograms.Generator;
using Model.Nonograms.Solver;
using Model.Utility;

namespace ConsoleApplication.Commands;

public class NonogramGenerateBatchCommand : GenerateBatchWithRandomSizeCommand<Nonogram, INonogramSolvingState>
{
    public NonogramGenerateBatchCommand() : base("Nonogram", new RandomNonogramGenerator())
    {
    }

    protected override ITrackerAttachableSolver<INonogramSolvingState> GetSolver(ArgumentInterpreter interpreter)
    { 
        return interpreter.Instantiator.InstantiateNonogramSolver();
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