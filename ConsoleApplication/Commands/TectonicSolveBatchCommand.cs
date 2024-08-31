using ConsoleApplication.Commands.Abstracts;
using Model.Core;
using Model.Tectonics;
using Model.Tectonics.Solver;

namespace ConsoleApplication.Commands;

public class TectonicSolveBatchCommand : SolveBatchCommand<INumericSolvingState>
{
    public TectonicSolveBatchCommand() : base("Tectonic")
    {
    }

    protected override ITrackerAttachableSolver<INumericSolvingState> GetSolver(Instantiator instantiator)
    {
        return instantiator.InstantiateTectonicSolver();
    }

    protected override bool Set(ISolver solver, string asString)
    {
        var tectonic = TectonicTranslator.GuessFormat(asString) switch
        {
            TectonicStringFormat.Code => TectonicTranslator.TranslateCodeFormat(asString),
            TectonicStringFormat.Rd => TectonicTranslator.TranslateRdFormat(asString),
            _ => null
        };

        if (tectonic is null) return false;

        ((TectonicSolver)solver).SetTectonic(tectonic);
        return true;
    }

    protected override string CurrentToString(ISolver solver)
    {
        return TectonicTranslator.TranslateRdFormat(((TectonicSolver)solver).Tectonic);
    }
}