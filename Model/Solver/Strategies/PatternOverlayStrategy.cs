using System;
using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class PatternOverlayStrategy : AbstractStrategy
{
    public const string OfficialName = "Pattern Overlay";

    public PatternOverlayStrategy() : base(OfficialName, StrategyDifficulty.Extreme)
    {
    }
    
    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        var allPatterns = GetPatterns(strategyManager);

        for (int number = 1; number <= 9; number++)
        {
            if (SearchForElimination(strategyManager, number, allPatterns[number - 1])) return;
        }

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (i == j) continue;
                
                var p1 = allPatterns[i];
                var p2 = allPatterns[j];

                p1.RemoveAll(pattern1 =>
                {
                    bool isOk = false;

                    foreach (var pattern2 in p2)
                    {
                        if (!pattern1.PeakAny(pattern2))
                        {
                            isOk = true;
                            break;
                        }
                    }

                    return !isOk;
                });
            }
        }
        
        for (int number = 1; number <= 9; number++)
        {
            if (SearchForElimination(strategyManager, number, allPatterns[number - 1])) return;
        }
    }

    private bool SearchForElimination(IStrategyManager strategyManager, int number, List<GridPositions> patterns)
    {
        if (patterns.Count == 0) return false;
            
        var and = patterns[0].And(patterns);
        foreach (var cell in and)
        {
            strategyManager.ChangeBuffer.AddSolutionToAdd(number, cell.Row, cell.Col);
        }

        if (strategyManager.ChangeBuffer.Push(this, new PatternOverlayReportBuilder())) return true;
            
        var or = patterns[0].Or(patterns);
        var gp = strategyManager.PositionsFor(number);

        foreach (var cell in gp.Difference(or))
        {
            strategyManager.ChangeBuffer.AddPossibilityToRemove(number, cell.Row, cell.Col);
        }

        return strategyManager.ChangeBuffer.Push(this, new PatternOverlayReportBuilder());
    }

    private List<GridPositions>[] GetPatterns(IStrategyManager strategyManager)
    {
        List<GridPositions>[] result = new List<GridPositions>[9];

        for (int i = 0; i < 9; i++)
        {
            List<GridPositions> currentResult = new();

            SearchForPattern(strategyManager, new LinePositions(), new LinePositions(),
                new GridPositions(), i + 1, currentResult, 0);

            result[i] = currentResult;
        }

        return result;
    }

    private void SearchForPattern(IStrategyManager strategyManager, LinePositions colsUsed, LinePositions miniColsUsed,
        GridPositions current, int number, List<GridPositions> result, int row)
    {
        if (row == 9)
        {
            result.Add(current.Copy());
            return;
        }
        
        var cols = strategyManager.RowPositionsAt(row, number);
        LinePositions nextMCU;
        
        if (cols.Count != 0)
        {
            foreach (var col in cols)
            {
                if (colsUsed.Peek(col) || miniColsUsed.Peek(col)) continue;

                var cell = new Cell(row, col);
                current.Add(cell);
                
                colsUsed.Add(col);
                if ((row + 1) % 3 == 0) nextMCU = new LinePositions();
                else
                {
                    nextMCU = miniColsUsed.Copy();
                    nextMCU.FillMiniGrid(col / 3);
                }

                SearchForPattern(strategyManager, colsUsed, nextMCU, current, number, result, row + 1);

                current.Remove(cell);
                colsUsed.Remove(col);
            }
        }
        else
        {
            int col = 0;
            for (; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] == number) break;
            }

            colsUsed.Add(col);
            if ((row + 1) % 3 == 0) nextMCU = new LinePositions();
            else
            {
                nextMCU = miniColsUsed.Copy();
                nextMCU.FillMiniGrid(col / 3);
            }

            SearchForPattern(strategyManager, colsUsed, nextMCU, current, number, result, row + 1);
        }
    }
}

public class PatternOverlayReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes));
    }
}