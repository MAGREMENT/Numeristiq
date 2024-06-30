using Model.Core;

namespace Model.Nonograms.Solver.Strategies;

public class UnreachableSquareStrategy : Strategy<INonogramSolverData>
{
    public UnreachableSquareStrategy() : base("Unreachable Square", StepDifficulty.Medium, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        
    }
}