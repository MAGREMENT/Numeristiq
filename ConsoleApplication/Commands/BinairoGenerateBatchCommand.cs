using ConsoleApplication.Commands.Abstracts;
using Model.Binairos;
using Model.Core;
using Model.Core.Generators;
using Model.Utility;

namespace ConsoleApplication.Commands;

public class BinairoGenerateBatchCommand : GenerateBatchWithRandomSizeCommand<Binairo, IBinarySolvingState>
{
    public BinairoGenerateBatchCommand() : base("Binairo", new RDRBinairoPuzzleGenerator(
        new FilledBinairoPuzzleGenerator()))
    {
    }

    protected override ITrackerAttachableSolver<IBinarySolvingState> GetSolver(ArgumentInterpreter interpreter)
    {
        return interpreter.Instantiator.InstantiateBinairoSolver();
    }

    protected override GeneratedPuzzle<Binairo> CreateGeneratedPuzzle(Binairo puzzle)
    {
        return new GeneratedBinairoPuzzle(puzzle);
    }

    protected override void SetPuzzle(ISolver solver, Binairo puzzle)
    {
        ((BinairoSolver)solver).SetBinairo(puzzle);
    }

    protected override GridSizeRandomizer GetRandomizer(IPuzzleGenerator<Binairo> generator)
    {
        return ((FilledBinairoPuzzleGenerator)((RDRBinairoPuzzleGenerator)generator).FilledGenerator).Randomizer;
    }

    protected override bool CheckSizeOptionsValues(IEnumerable<(Option, int)> options)
    {
        foreach (var (option, value) in options)
        {
            if (value % 2 == 1)
            {
                Console.WriteLine("Option value need to be pair : " + option.Name);
                return false;
            }
        }

        return true;
    }
}