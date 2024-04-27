using Model.Helpers.Graphs;
using Model.Tectonics.Utility.ConstructRules;

namespace Model.Tectonics.Utility;

public class TectonicConstructRuleBank : IConstructRuleBank<ITectonicStrategyUser, ITectonicElement>
{
    public const int NeighborLink = 0,
        ZoneLink = 1,
        CellLink = 2;
    
    private readonly IConstructRule<ITectonicStrategyUser, ITectonicElement>[] _rules = 
    {
        new NeighborLinkConstructRule(),
        new ZoneLinkConstructRule(), 
        new CellLinkConstructRule()
    };

    public IConstructRule<ITectonicStrategyUser, ITectonicElement> this[int index] => _rules[index];
}