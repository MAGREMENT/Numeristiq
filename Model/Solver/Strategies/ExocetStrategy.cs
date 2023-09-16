using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class ExocetStrategy : IStrategy
{
    public string Name => "Exocet";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public StatisticsTracker Tracker { get; } = new();

    private readonly int _max;

    public ExocetStrategy(int max)
    {
        _max = max;
    }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        //Rows
        for (int row = 0; row < 9; row++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                for (int m1 = 0; m1 < 3; m1++)
                {
                    int col1 = gridCol * 3 + m1;
                    var poss1 = strategyManager.PossibilitiesAt(row, col1);
                    if (poss1.Count < 2 || poss1.Count > _max) continue;

                    for (int m2 = m1 + 1; m2 < 3; m2++)
                    {
                        int col2 = gridCol * 3 + m2;
                        var poss2 = strategyManager.PossibilitiesAt(row, col2);
                        if (poss2.Count < 2 || poss1.Count > _max) continue;

                        var or = poss1.Or(poss2);
                        if(or.Count > _max) continue;

                        List<Cell>[] possibleTargets = { new(), new() };

                        int startRow = row / 3 * 3;
                        int cursor = 0;

                        for (int miniRow = 0; miniRow < 3; miniRow++)
                        {
                            int checkedRow = startRow + miniRow;
                            if (checkedRow == row) continue;

                            for (int col = 0; col < 9; col++)
                            {
                                if (col / 3 == gridCol) continue;

                                var checkedPos = strategyManager.PossibilitiesAt(checkedRow, col);
                                if (checkedPos.PeekAll(or)) possibleTargets[cursor].Add(new Cell(checkedRow, col));
                            }

                            cursor++;
                        }

                        if (possibleTargets[0].Count == 0 || possibleTargets[1].Count == 0) continue;

                        foreach (var target1 in possibleTargets[0])
                        {
                            foreach (var target2 in possibleTargets[1])
                            {
                                if(target1.ShareAUnit(target2) 
                                   || strategyManager.PossibilitiesAt(target1.Row, target2.Col).PeekAny(or)
                                   || strategyManager.PossibilitiesAt(target2.Row, target1.Col).PeekAny(or)) continue;
                                
                                
                            }
                        }
                    }
                }
            }
        }
    }
}