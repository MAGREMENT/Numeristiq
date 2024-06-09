using Model.Core;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Solver.Utility;

namespace Model.Tectonics.Solver.Strategies.AlternatingInference.Types;

public class AlternatingInferenceChainType : IAlternatingInferenceType
{
    public string Name => "Alternating Inference Chains";

    public StepDifficulty Difficulty => StepDifficulty.Extreme;
    
    public ILinkGraph<ITectonicElement> GetGraph(ITectonicStrategyUser strategyUser)
    {
        strategyUser.Graphs.ConstructComplex(TectonicConstructRuleBank.ZoneLink,
            TectonicConstructRuleBank.CellLink, TectonicConstructRuleBank.NeighborLink);
        return strategyUser.Graphs.ComplexLinkGraph;
    }
}