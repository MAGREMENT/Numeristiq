using Model.Core;
using Model.Tectonics.Solver;
using Model.Tectonics.Solver.Strategies;
using Model.Tectonics.Solver.Strategies.AlternatingInference;
using Model.Tectonics.Solver.Strategies.AlternatingInference.Types;

namespace Repository.HardCoded;

public class HardCodedTectonicStrategyRepository : HardCodedStrategyRepository<Strategy<ITectonicSolverData>>
{ 
    public override IEnumerable<Strategy<ITectonicSolverData>> GetStrategies()
    {
        yield return new NakedSingleStrategy();
        yield return new HiddenSingleStrategy();
        yield return new NakedDoubleStrategy();
        yield return new HiddenDoubleStrategy();
        yield return new ZoneInteractionStrategy();
        yield return new AlternatingInferenceGeneralization(new XChainType());
        yield return new GroupEliminationStrategy();
        yield return new AlternatingInferenceGeneralization(new AlternatingInferenceChainType());
        yield return new BruteForceStrategy();
    }
}