using Model.Core;

namespace Model.Nonograms.Solver.Strategies;

public class ValueCompletionStrategy : Strategy<INonogramSolverData>
{
    public ValueCompletionStrategy() : base("Value Completion", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int col = 0; col < data.Nonogram.ColumnCount; col++)
        {
            var space = data.PreComputer.VerticalMainSpace(col);
            if (space.IsInvalid() || space.GetValueCount() == 0) continue;

            for (int r = space.Start; r < space.End; r++)
            {
                if(data.IsAvailable(r, col)) continue;

                if (data.Nonogram[r, col])
                {
                    var target = data.Nonogram.VerticalLineCollection.TryGetValue(col, space.FirstValueIndex);
                    //TODO
                }
            }
        }
    }
}