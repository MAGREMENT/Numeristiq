using ConsoleApplication.Commands.Abstracts;
using Model.Binairos;
using Model.Core;

namespace ConsoleApplication.Commands;

public class BinairoSolveBatchCommand : SolveBatchCommand<IBinarySolvingState>
{
    public BinairoSolveBatchCommand() : base("Binairo")
    {
    }

    protected override ITrackerAttachableSolver<IBinarySolvingState> GetSolver(Instantiator instantiator)
    {
        return instantiator.InstantiateBinairoSolver();
    }

    protected override bool Set(ISolver solver, string asString)
    {
        ((BinairoSolver)solver).SetBinairo(BinairoTranslator.TranslateLineFormat(asString));
        return true;
    }

    protected override string CurrentToString(ISolver solver)
    {
        return BinairoTranslator.TranslateLineFormat(((BinairoSolver)solver).Binairo);
    }
}