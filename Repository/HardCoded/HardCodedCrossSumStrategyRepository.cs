using Model.Core;
using Model.CrossSums.Solver;
using Model.CrossSums.Solver.Strategies;

namespace Repository.HardCoded;

public class HardCodedCrossSumStrategyRepository : HardCodedStrategyRepository<Strategy<ICrossSumSolverData>>
{ 
    public override IEnumerable<Strategy<ICrossSumSolverData>> GetStrategies()
    {
        yield return new PerfectCountStrategy();
        yield return new TooBigStrategy();
        yield return new SingleOddStrategy(); 
        yield return new NotEnoughWithoutStrategy();
    }
}