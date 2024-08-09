using Model.Core;
using Model.Nonograms.Solver;
using Model.Nonograms.Solver.Strategies;

namespace Repository;

public class HardCodedNonogramStrategyRepository : HardCodedStrategyRepository<Strategy<INonogramSolverData>>
{ 
    public override IEnumerable<Strategy<INonogramSolverData>> GetStrategies()
    {
        yield return new PerfectRemainingSpaceStrategy();
        yield return new NotEnoughSpaceStrategy();
        yield return new EdgeValueStrategy(); 
        yield return new PerfectValueSpaceStrategy();
        yield return new ValueCompletionStrategy();
        yield return new BridgingStrategy();
        yield return new SplittingStrategy();
        yield return new ValueOverlayStrategy();
        yield return new UnreachableSquaresStrategy();
        yield return new BruteForceStrategy();
    }
}