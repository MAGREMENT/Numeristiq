using System.Collections.Generic;
using System.Linq;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;

namespace Model.Solver.Strategies;

public class FinnedXWingStrategy : IStrategy
{
    public string Name => "Finned XWing";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public StatisticsTracker Tracker { get; } = new();
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            Dictionary<int, IReadOnlyLinePositions> candidates = new();
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.RowPositionsAt(row, number);
                if (ppir.Count == 2) candidates.Add(row, ppir);
            }

            foreach (var entry in candidates)
            {
                for (int row = 0; row < 9; row++)
                {
                    if (row == entry.Key) continue;
                    var currentPpir = strategyManager.RowPositionsAt(row, number);

                    var candidatePositions = entry.Value.ToArray();

                    if (currentPpir.Peek(candidatePositions[0]) &&
                        HasSameMiniCol(currentPpir, candidatePositions[1], candidatePositions[0]))
                    {
                        ProcessRow(strategyManager, entry.Key,row, candidatePositions[1],
                            number);

                        strategyManager.ChangeBuffer.Push(this,
                            new FinnedXWingReportBuilder(entry.Value, entry.Key, currentPpir,
                                row, number, Unit.Row));
                    }


                    if (currentPpir.Peek(candidatePositions[1]) &&
                        HasSameMiniCol(currentPpir, candidatePositions[0], candidatePositions[1]))
                    {
                        ProcessRow(strategyManager, entry.Key,row, candidatePositions[0],
                            number);

                        strategyManager.ChangeBuffer.Push(this,
                            new FinnedXWingReportBuilder(entry.Value, entry.Key, currentPpir,
                                row, number, Unit.Row));
                    }
                        
                }
            }
            
            candidates.Clear();
            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.ColumnPositionsAt(col, number);
                if (ppic.Count == 2) candidates.Add(col, ppic);
            }

            foreach (var entry in candidates)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (col == entry.Key) continue;
                    var currentPpic = strategyManager.ColumnPositionsAt(col, number);

                    var candidatePositions = entry.Value.ToArray();

                    if (currentPpic.Peek(candidatePositions[0]) &&
                        HasSameMiniRow(currentPpic, candidatePositions[1], candidatePositions[0]))
                    {
                        ProcessColumn(strategyManager, entry.Key,col, candidatePositions[1] ,
                            number);

                        strategyManager.ChangeBuffer.Push(this,
                            new FinnedXWingReportBuilder(entry.Value, entry.Key, currentPpic,
                                col, number, Unit.Column));
                    }


                    if (currentPpic.Peek(candidatePositions[1]) &&
                        HasSameMiniRow(currentPpic, candidatePositions[0], candidatePositions[1]))
                    {
                        ProcessColumn(strategyManager, entry.Key,col, candidatePositions[0],
                            number);

                        strategyManager.ChangeBuffer.Push(this,
                            new FinnedXWingReportBuilder(entry.Value, entry.Key, currentPpic,
                                col, number, Unit.Column));
                    }
                }
            }
        }
    }

    private bool HasSameMiniCol(IReadOnlyLinePositions toExamine, int col, int except)
    {
        var miniCol = col / 3;
        foreach (var colToExamine in toExamine)
        {
            if(colToExamine == except) continue;
            if (colToExamine / 3 != miniCol) return false;
        }

        return true;
    }

    private void ProcessRow(IStrategyManager strategyManager, int normalRow, int finnedRow, int finnedCol, int number)
    {
        var startRow = finnedRow / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            int row = startRow + i;
            if (row == normalRow || row == finnedRow) continue;
            strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, finnedCol);
        }
    }
    
    private bool HasSameMiniRow(IReadOnlyLinePositions toExamine, int row, int except)
    {
        var miniRow = row / 3;
        foreach (var rowToExamine in toExamine)
        {
            if(rowToExamine == except) continue;
            if (rowToExamine / 3 != miniRow) return false;
        }

        return true;
    }
    
    private void ProcessColumn(IStrategyManager strategyManager, int normalCol, int finnedCol, int finnedRow, int number)
    {
        var startCol = finnedCol / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            int col = startCol + i;
            if (col == finnedCol || col == normalCol) continue;
            strategyManager.ChangeBuffer.AddPossibilityToRemove(number, finnedRow, col);
        }
    }
}

public class FinnedXWingReportBuilder : IChangeReportBuilder
{
    private readonly IReadOnlyLinePositions _normal;
    private readonly int _normalUnit;
    private readonly IReadOnlyLinePositions _finned;
    private readonly int _finnedUnit;
    private readonly int _number;
    private readonly Unit _unit;

    public FinnedXWingReportBuilder(IReadOnlyLinePositions normal, int normalUnit, IReadOnlyLinePositions finned, int finnedUnit,
        int number, Unit unit)
    {
        _normal = normal;
        _finned = finned;
        _number = number;
        _normalUnit = normalUnit;
        _finnedUnit = finnedUnit;
        _unit = unit;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var normalOther in _normal)
            {
                if(_unit == Unit.Row) lighter.HighlightPossibility(_number, _normalUnit,
                    normalOther, ChangeColoration.CauseOffOne);
                else lighter.HighlightPossibility(_number, normalOther,
                    _normalUnit, ChangeColoration.CauseOffOne);
            }

            foreach (var finnedOther in _finned)
            {
                if (_unit == Unit.Row)
                    lighter.HighlightPossibility(_number, _finnedUnit, finnedOther,
                        _normal.Peek(finnedOther) ? ChangeColoration.CauseOffOne : ChangeColoration.CauseOffTwo);
                else
                    lighter.HighlightPossibility(_number, finnedOther, _finnedUnit,
                        _normal.Peek(finnedOther) ? ChangeColoration.CauseOffOne : ChangeColoration.CauseOffTwo);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}