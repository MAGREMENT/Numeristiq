using Model.Core;
using Model.Core.Changes;
using Model.Kakuros;

namespace Model.Nonograms.Strategies;

public class PerfectSpaceStrategy : Strategy<INonogramSolverData>
{
    public PerfectSpaceStrategy() : base("Perfect Space", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var spaceNeeded = data.Nonogram.HorizontalLineCollection.SpaceNeeded(row);
            foreach (var space in data.EnumerateSpaces(Orientation.Horizontal, row))
            {
                if (spaceNeeded != space.GetLength) continue;
                
                foreach (var cell in space.EnumerateCells(Orientation.Horizontal, row))
                {
                    data.ChangeBuffer.ProposePossibilityRemoval(cell);
                }
                    
                if (data.ChangeBuffer.NotEmpty() && data.ChangeBuffer.Commit(DefaultDichotomousChangeReportBuilder<
                        IUpdatableDichotomousSolvingState, object>.Instance) && StopOnFirstPush) return;
                break;
            }
        }
        
        //TODO cols
    }
}