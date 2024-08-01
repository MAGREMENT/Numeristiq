using Model.Core;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Solver.Utility;

namespace Model.Tectonics.Solver.Strategies.AlternatingInference.Types;

public class XChainType : IAlternatingInferenceType
{
    public string Name => "X-Chains";

    public Difficulty Difficulty => Difficulty.Hard;
    
    public ILinkGraph<ITectonicElement> GetGraph(ITectonicSolverData solverData)
    {
        solverData.Graphs.ConstructComplex(TectonicConstructRuleBank.ZoneLink,
            TectonicConstructRuleBank.NeighborLink);
        return solverData.Graphs.ComplexLinkGraph;
    }
}