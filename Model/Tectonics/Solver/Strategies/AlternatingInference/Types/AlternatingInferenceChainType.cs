using Model.Core;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Solver.Utility;
using Model.Tectonics.Solver.Utility.ConstructRules;

namespace Model.Tectonics.Solver.Strategies.AlternatingInference.Types;

public class AlternatingInferenceChainType : IAlternatingInferenceType
{
    public string Name => "Alternating Inference Chains";

    public Difficulty Difficulty => Difficulty.Extreme;
    
    public ILinkGraph<ITectonicElement> GetGraph(ITectonicSolverData solverData)
    {
        solverData.ManagedGraph.Construct(ZoneLinkConstructRule.Instance,
            CellLinkConstructRule.Instance, NeighborLinkConstructRule.Instance);
        return solverData.ManagedGraph.Graph;
    }
}