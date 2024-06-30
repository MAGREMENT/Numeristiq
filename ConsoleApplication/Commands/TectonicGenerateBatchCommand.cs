using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Core.Generators;
using Model.Core.Trackers;
using Model.Tectonics;
using Model.Tectonics.Generator;
using Model.Tectonics.Solver;
using Model.Utility;

namespace ConsoleApplication.Commands;

public class TectonicGenerateBatchCommand : GenerateBatchWithRandomSizeCommand<ITectonic, INumericSolvingState>
{ 
    public TectonicGenerateBatchCommand() : base("Tectonic", new RDRTectonicPuzzleGenerator(
        new RandomLayoutBackTrackingFilledTectonicGenerator()))
    {
    }

    protected override ITrackerAttachableSolver<INumericSolvingState> GetSolver(ArgumentInterpreter interpreter)
    {
        return interpreter.Instantiator.InstantiateTectonicSolver();
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