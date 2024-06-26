using Model.Core;

namespace Model.Nonograms.Solver.Strategies;

public class SingleValueOverlayStrategy : Strategy<INonogramSolverData>
{
    public SingleValueOverlayStrategy() : base("Single Value Overlay", StepDifficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        throw new System.NotImplementedException();
    }
}