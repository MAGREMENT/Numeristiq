using Model.Core.Graphs;
using Model.Tectonics.Solver.Utility.ConstructRules;

namespace Model.Tectonics.Solver.Utility;

public class TectonicConstructRuleBank : IConstructRuleBank<ITectonicSolverData, ITectonicElement>
{
    public const int NeighborLink = 0,
        ZoneLink = 1,
        CellLink = 2;
    
    private readonly IConstructRule<ITectonicSolverData, ITectonicElement>[] _rules = 
    {
        new NeighborLinkConstructRule(),
        new ZoneLinkConstructRule(), 
        new CellLinkConstructRule()
    };

    public IConstructRule<ITectonicSolverData, ITectonicElement> this[int index] => _rules[index];
}