using Model.Core;
using Model.Kakuros;

namespace Model.Nonograms.Strategies;

public class NotEnoughSpaceStrategy : Strategy<INonogramSolverData>
{
    public NotEnoughSpaceStrategy() : base("Perfect Space", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(INonogramSolverData data)
    {
        for (int row = 0; row < data.Nonogram.RowCount; row++)
        {
            var spaceNeeded = data.Nonogram.HorizontalLineCollection.SpaceNeeded(row);
            foreach (var space in data.EnumerateSpaces(Orientation.Horizontal, row))
            {
                if (spaceNeeded <= space.GetLength) continue;
                
                foreach (var cell in space.EnumerateCells(Orientation.Horizontal, row))
                {
                    //TODO propose
                }
                    
                //TODO commit
                break;
            }
        }
        
        //TODO cols
    }
}