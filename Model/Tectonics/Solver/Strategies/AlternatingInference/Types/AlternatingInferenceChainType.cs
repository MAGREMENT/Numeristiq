using Model.Core;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Solver.Utility;

namespace Model.Tectonics.Solver.Strategies.AlternatingInference.Types;

public class AlternatingInferenceChainType : IAlternatingInferenceType
{
    public string Name => "Alternating Inference Chains";

    public Difficulty Difficulty => Difficulty.Extreme;
    
    public ILinkGraph<ITectonicElement> GetGraph(ITectonicSolverData solverData)
    {
        solverData.Graphs.ConstructComplex(TectonicConstructRuleBank.ZoneLink,
            TectonicConstructRuleBank.CellLink, TectonicConstructRuleBank.NeighborLink);
        return solverData.Graphs.ComplexLinkGraph;
    }
}