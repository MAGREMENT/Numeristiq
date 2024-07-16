using ConsoleApplication.Commands.Abstracts;
using Model.Core.BackTracking;
using Model.Tectonics;

namespace ConsoleApplication.Commands;

public class TectonicSolutionCountCommand : SolutionCountCommand<ITectonic, IPossibilitiesGiver>
{
    public TectonicSolutionCountCommand() : base("Tectonic")
    {
    }

    protected override BackTracker<ITectonic, IPossibilitiesGiver> BackTracker { get; } = new TectonicBackTracker();
    protected override void SetBackTracker(string s)
    {
        var t = TectonicTranslator.TranslateRdFormat(s);
        BackTracker.Set(t, new TectonicPossibilitiesGiver(t));
    }

    protected override string ToString(ITectonic puzzle)
    {
        return TectonicTranslator.TranslateRdFormat(puzzle);
    }
}