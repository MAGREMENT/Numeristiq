using System;
using Model.Solver.Position;
using Model.Solver.Possibility;

namespace Model.Solver.Strategies;

public class ReverseBUGLiteStrategy : AbstractStrategy
{
    public const string OfficialName = "Reverse BUG-Lite";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public ReverseBUGLiteStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
    }
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int u1 = 0; u1 < 8; u1++)
        {
            for (int u2 = u1 + 1; u2 < 9; u2++)
            {
                //Rows
                LinePositions one = new LinePositions();
                LinePositions two = new LinePositions();
                
                Possibilities onePoss = Possibilities.NewEmpty();
                Possibilities twoPoss = Possibilities.NewEmpty();
                
                for (int o = 0; o < 9; o++)
                {
                    var solved = strategyManager.Sudoku[u1, o];
                    if (solved != 0)
                    {
                        one.Add(o);
                        onePoss.Add(solved);
                    }
                    
                    solved = strategyManager.Sudoku[u2, o];
                    if (solved != 0)
                    {
                        two.Add(o);
                        twoPoss.Add(solved);
                    }
                }

                if (Math.Abs(onePoss.Count - twoPoss.Count) != 1 || !onePoss.PeekAny(twoPoss)) continue;
                
                var orPossibilities = onePoss.Or(twoPoss);
                if (orPossibilities.Count > 3) continue;

                var orPositions = one.Or(two);
                if (orPositions.Count != orPossibilities.Count ||
                    orPositions.MiniGridCount() != orPossibilities.Count) continue;
            }
        }
    }
}