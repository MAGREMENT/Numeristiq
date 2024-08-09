using Model.Core;
using Model.Kakuros;
using Model.Kakuros.Strategies;

namespace Repository;

public class HardCodedKakuroStrategyRepository : HardCodedStrategyRepository<Strategy<IKakuroSolverData>>
{
    public override IEnumerable<Strategy<IKakuroSolverData>> GetStrategies()
    {
        yield return new NakedSingleStrategy();
        yield return new AmountCoherencyStrategy();
        yield return new CombinationCoherencyStrategy();
    }
}