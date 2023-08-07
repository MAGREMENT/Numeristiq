using System.Collections.Generic;
using System.Linq;
using Model.Positions;

namespace Model.Strategies;

public class FinnedXWingStrategy : IStrategy
{
    public string Name => "Finned XWing";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            Dictionary<int, LinePositions> candidates = new();
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.PossibilityPositionsInRow(row, number);
                if (ppir.Count == 2) candidates.Add(row, ppir);
            }

            foreach (var entry in candidates)
            {
                for (int row = 0; row < 9; row++)
                {
                    if (row == entry.Key) continue;
                    var currentPpir = strategyManager.PossibilityPositionsInRow(row, number);

                    var candidatePositions = entry.Value.ToArray();

                    if (currentPpir.Peek(candidatePositions[0]) &&
                        HasSameMiniCol(currentPpir, candidatePositions[1], candidatePositions[0]))
                    {
                        var changeBuffer = strategyManager.CreateChangeBuffer(this,
                            new FinnedXWingReportWaiter(entry.Value, entry.Key, currentPpir,
                                row, number, Unit.Row));
                        
                        ProcessRow(changeBuffer, entry.Key,row, candidatePositions[1],
                            number);

                        changeBuffer.Push();
                    }


                    if (currentPpir.Peek(candidatePositions[1]) &&
                        HasSameMiniCol(currentPpir, candidatePositions[0], candidatePositions[1]))
                    {
                        var changeBuffer = strategyManager.CreateChangeBuffer(this,
                            new FinnedXWingReportWaiter(entry.Value, entry.Key, currentPpir,
                                row, number, Unit.Row));
                        
                        ProcessRow(changeBuffer, entry.Key,row, candidatePositions[0],
                            number);

                        changeBuffer.Push();
                    }
                        
                }
            }
            
            candidates.Clear();
            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.PossibilityPositionsInColumn(col, number);
                if (ppic.Count == 2) candidates.Add(col, ppic);
            }

            foreach (var entry in candidates)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (col == entry.Key) continue;
                    var currentPpic = strategyManager.PossibilityPositionsInColumn(col, number);

                    var candidatePositions = entry.Value.ToArray();

                    if (currentPpic.Peek(candidatePositions[0]) &&
                        HasSameMiniRow(currentPpic, candidatePositions[1], candidatePositions[0]))
                    {
                        var changeBuffer = strategyManager.CreateChangeBuffer(this,
                            new FinnedXWingReportWaiter(entry.Value, entry.Key, currentPpic,
                                col, number, Unit.Column));
                        
                        ProcessColumn(changeBuffer, entry.Key,col, candidatePositions[1] ,
                            number);

                        changeBuffer.Push();
                    }


                    if (currentPpic.Peek(candidatePositions[1]) &&
                        HasSameMiniRow(currentPpic, candidatePositions[0], candidatePositions[1]))
                    {
                        var changeBuffer = strategyManager.CreateChangeBuffer(this,
                            new FinnedXWingReportWaiter(entry.Value, entry.Key, currentPpic,
                                col, number, Unit.Column));
                        
                        ProcessColumn(changeBuffer, entry.Key,col, candidatePositions[0],
                            number);

                        changeBuffer.Push();
                    }
                }
            }
        }
    }

    private bool HasSameMiniCol(LinePositions toExamine, int col, int except)
    {
        var miniCol = col / 3;
        foreach (var colToExamine in toExamine)
        {
            if(colToExamine == except) continue;
            if (colToExamine / 3 != miniCol) return false;
        }

        return true;
    }

    private void ProcessRow(ChangeBuffer changeBuffer, int normalRow, int finnedRow, int finnedCol, int number)
    {
        var startRow = finnedRow / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            int row = startRow + i;
            if (row == normalRow || row == finnedRow) continue;
            changeBuffer.AddPossibilityToRemove(number, row, finnedCol);
        }
    }
    
    private bool HasSameMiniRow(LinePositions toExamine, int row, int except)
    {
        var miniRow = row / 3;
        foreach (var rowToExamine in toExamine)
        {
            if(rowToExamine == except) continue;
            if (rowToExamine / 3 != miniRow) return false;
        }

        return true;
    }
    
    private void ProcessColumn(ChangeBuffer changeBuffer, int normalCol, int finnedCol, int finnedRow, int number)
    {
        var startCol = finnedCol / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            int col = startCol + i;
            if (col == finnedCol || col == normalCol) continue;
            changeBuffer.AddPossibilityToRemove(number, finnedRow, col);
        }
    }
}

public class FinnedXWingReportWaiter : IChangeReportWaiter
{
    private readonly LinePositions _normal;
    private readonly int _normalUnit;
    private readonly LinePositions _finned;
    private readonly int _finnedUnit;
    private readonly int _number;
    private readonly Unit _unit;

    public FinnedXWingReportWaiter(LinePositions normal, int normalUnit, LinePositions finned, int finnedUnit,
        int number, Unit unit)
    {
        _normal = normal;
        _finned = finned;
        _number = number;
        _normalUnit = normalUnit;
        _finnedUnit = finnedUnit;
        _unit = unit;
    }

    public ChangeReport Process(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportWaiter.ChangesToString(changes), lighter =>
        {
            foreach (var normalOther in _normal)
            {
                if(_unit == Unit.Row) lighter.HighLightPossibility(_number, _normalUnit,
                    normalOther, ChangeColoration.CauseOffOne);
                else lighter.HighLightPossibility(_number, normalOther,
                    _normalUnit, ChangeColoration.CauseOffOne);
            }

            foreach (var finnedOther in _finned)
            {
                if (_unit == Unit.Row)
                    lighter.HighLightPossibility(_number, _finnedUnit, finnedOther,
                        _normal.Peek(finnedOther) ? ChangeColoration.CauseOffOne : ChangeColoration.CauseOffTwo);
                else
                    lighter.HighLightPossibility(_number, finnedOther, _finnedUnit,
                        _normal.Peek(finnedOther) ? ChangeColoration.CauseOffOne : ChangeColoration.CauseOffTwo);
            }
            
            IChangeReportWaiter.HighLightChanges(lighter, changes);
        }, "");
    }
}