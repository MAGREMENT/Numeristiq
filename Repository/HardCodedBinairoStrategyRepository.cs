using Model.Binairos;
using Model.Binairos.Strategies;
using Model.Core;

namespace Repository;

public class HardCodedBinairoStrategyRepository : HardCodedStrategyRepository<Strategy<IBinairoSolverData>>
{
    public override IEnumerable<Strategy<IBinairoSolverData>> GetStrategies()
    {
        yield return new DoubleStrategy();
        yield return new TripleDenialStrategy();
        yield return new HalfCompletionStrategy();
    }
}