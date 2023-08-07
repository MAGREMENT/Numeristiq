using System.Collections.Generic;

namespace Model.Strategies;

public class BUGStrategy : IStrategy
{
    public string Name { get; } = "BUG";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Medium;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        int[]? triple = OnlyDoublesAndOneTriple(strategyManager);
        if (triple is not null)
        {
            var changeBuffer = strategyManager.CreateChangeBuffer(this, new BUGReportWaiter(triple));
            foreach (var possibility in strategyManager.Possibilities[triple[0], triple[1]])
            {
                if (strategyManager.PossibilityPositionsInColumn(triple[1], possibility).Count == 3 &&
                    strategyManager.PossibilityPositionsInRow(triple[0], possibility).Count == 3 &&
                    strategyManager.PossibilityPositionsInMiniGrid(triple[0] / 3, triple[1] / 3, possibility).Count == 3)
                {
                    changeBuffer.AddDefinitiveToAdd(possibility, triple[0], triple[1]);
                    break;
                }
            }

            changeBuffer.Push();
        }
    }

    private int[]? OnlyDoublesAndOneTriple(IStrategyManager strategyManager)
    {
        int[]? triple = null;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] == 0 && strategyManager.Possibilities[row, col].Count != 2)
                {
                    if (strategyManager.Possibilities[row, col].Count != 3 || triple is not null) return null;
                    triple = new[] { row, col };
                }
            }
        }

        return triple;
    }
}

public class BUGReportWaiter : IChangeReportWaiter
{
    private readonly int[] _triple;

    public BUGReportWaiter(int[] triple)
    {
        _triple = triple;
    }
    
    public ChangeReport Process(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportWaiter.ChangesToString(changes),
            lighter => IChangeReportWaiter.HighLightChanges(lighter, changes), "");
    }
}