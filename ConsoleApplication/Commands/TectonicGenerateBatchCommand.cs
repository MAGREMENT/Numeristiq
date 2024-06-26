using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Core.Generators;
using Model.Core.Trackers;
using Model.Tectonics;
using Model.Tectonics.Generator;
using Model.Tectonics.Solver;
using Model.Utility;

namespace ConsoleApplication.Commands;

public class TectonicGenerateBatchCommand : GenerateBatchWithRandomSizeCommand<ITectonic>
{ 
    public TectonicGenerateBatchCommand() : base("Tectonic", new RDRTectonicPuzzleGenerator(
        new RandomLayoutBackTrackingFilledTectonicGenerator()))
    {
    }

    protected override (ISolver, IRatingTracker, IHardestStrategyTracker) GetSolverWithAttachedTracker(ArgumentInterpreter interpreter)
    {
        var solver = interpreter.Instantiator.InstantiateTectonicSolver();
        var ratings = new RatingTracker();
        var hardest = new HardestStrategyTracker();

        ratings.AttachTo(solver);
        hardest.AttachTo(solver);

        return (solver, ratings, hardest);
    }

    protected override GeneratedPuzzle<ITectonic> CreateGeneratedPuzzle(ITectonic puzzle)
    {
        return new GeneratedTectonicPuzzle(puzzle);
    }

    protected override void SetPuzzle(ISolver solver, ITectonic puzzle)
    {
        ((TectonicSolver)solver).SetTectonic(puzzle);
    }

    protected override GridSizeRandomizer GetRandomizer(IPuzzleGenerator<ITectonic> generator)
    {
        var filledGenerator = ((RDRTectonicPuzzleGenerator)generator).FilledGenerator;
        return ((RandomLayoutBackTrackingFilledTectonicGenerator)filledGenerator).Randomizer;
    }
}