using Model.Core;

namespace Model.Nonograms.Solver.Strategies;

public class EdgeLogicStrategy : Strategy<INonogramSolverData>
{
    public EdgeLogicStrategy() : base("Edge Logic", Difficulty.Hard, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        
    }
}