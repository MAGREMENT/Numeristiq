using Model.Core;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Solver.Utility;
using Model.Tectonics.Solver.Utility.ConstructRules;

namespace Model.Tectonics.Solver.Strategies.AlternatingInference.Types;

public class XChainType : IAlternatingInferenceType
{
    public string Name => "X-Chains";

    public Difficulty Difficulty => Difficulty.Hard;
    
    public IGraph<ITectonicElement, LinkStrength> GetGraph(ITectonicSolverData solverData)
    {
        solverData.ManagedGraph.Construct(ZoneLinkConstructionRule.Instance,
            NeighborLinkConstructionRule.Instance);
        return solverData.ManagedGraph.Graph;
    }
}