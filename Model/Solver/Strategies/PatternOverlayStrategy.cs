using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class PatternOverlayStrategy : AbstractStrategy
{
    public const string OfficialName = "Pattern Overlay";

    private readonly int _max;

    public PatternOverlayStrategy(int max) : base(OfficialName, StrategyDifficulty.Extreme)
    {
        _max = max;
    }
    
    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        var patterns = GetPatterns(strategyManager);
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

            current.Add(new Cell(row, col));
            
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