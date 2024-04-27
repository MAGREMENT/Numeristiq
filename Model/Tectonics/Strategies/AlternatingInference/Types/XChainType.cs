using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Utility;

namespace Model.Tectonics.Strategies.AlternatingInference.Types;

public class XChainType : IAlternatingInferenceType
{
    public string Name => "X-Chains";

    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    
    public ILinkGraph<ITectonicElement> GetGraph(ITectonicStrategyUser strategyUser)
    {
        strategyUser.Graphs.ConstructComplex(TectonicConstructRuleBank.ZoneLink,
            TectonicConstructRuleBank.NeighborLink);
        return strategyUser.Graphs.ComplexLinkGraph;
    }
}