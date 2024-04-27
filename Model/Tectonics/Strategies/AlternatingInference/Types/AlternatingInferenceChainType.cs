using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Utility;

namespace Model.Tectonics.Strategies.AlternatingInference.Types;

public class AlternatingInferenceChainType : IAlternatingInferenceType
{
    public string Name => "Alternating Inference Chains";

    public StrategyDifficulty Difficulty => StrategyDifficulty.Extreme;
    
    public ILinkGraph<ITectonicElement> GetGraph(ITectonicStrategyUser strategyUser)
    {
        strategyUser.Graphs.ConstructComplex(TectonicConstructRuleBank.ZoneLink,
            TectonicConstructRuleBank.CellLink, TectonicConstructRuleBank.NeighborLink);
        return strategyUser.Graphs.ComplexLinkGraph;
    }
}