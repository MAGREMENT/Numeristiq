using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Nonograms;
using Model.Nonograms.Solver;

namespace ConsoleApplication.Commands;

public class NonogramSolveBatchCommand : SolveBatchCommand<INonogramSolvingState>
{
    public NonogramSolveBatchCommand() : base("Nonogram")
    {
    }

    protected override ITrackerAttachableSolver<INonogramSolvingState> GetSolver(Instantiator instantiator)
    {
        return instantiator.InstantiateNonogramSolver();
    }

    protected override bool Set(ISolver solver, string asString)
    {
        ((NonogramSolver)solver).SetNonogram(NonogramTranslator.TranslateLineFormat(asString));
        return true;
    }

    protected override string CurrentToString(ISolver solver)
    {
        return NonogramTranslator.TranslateLineFormat(((NonogramSolver)solver).Nonogram);
    }
}