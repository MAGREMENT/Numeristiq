using System.Collections.Generic;
using System.Linq;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Position;

namespace Model.Sudoku.Solver.Strategies;

public class FinnedXWingStrategy : AbstractStrategy
{
    public const string OfficialName = "Finned X-Wing";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public FinnedXWingStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior){}
    
    public override void Apply(IStrategyUser strategyUser)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row1 = 0; row1 < 9; row1++)
            {
                var ppir1 = strategyUser.RowPositionsAt(row1, number);
                if (ppir1.Count < 2) continue;

                for (int row2 = row1 + 1; row2 < 9; row2++)
                {
                    var ppir2 = strategyUser.RowPositionsAt(row2, number);
                    if (ppir2.Count < 2) continue;

                    if (ppir1.Count == 2 && ExamineRow(strategyUser, row1, row2, ppir1,
                            ppir2, number)) return;
                    if (ppir2.Count == 2 && ExamineRow(strategyUser, row2, row1, ppir2,
                            ppir1, number)) return;
                }
            }

            for (int col1 = 0; col1 < 9; col1++)
            {
                var ppic1 = strategyUser.ColumnPositionsAt(col1, number);
                if (ppic1.Count < 2) continue;
                
                for (int col2 = col1 + 1; col2 < 9; col2++)
                {
                    var ppic2 = strategyUser.ColumnPositionsAt(col2, number);
                    if(ppic2.Count < 2) continue;

                    if (ppic1.Count == 2 && ExamineColumn(strategyUser, col1, col2, ppic1,
                            ppic2, number)) return;
                    if (ppic2.Count == 2 && ExamineColumn(strategyUser, col2, col1, ppic2,
                            ppic1, number)) return;
                }
            }
        }
    }

    private bool ExamineRow(IStrategyUser strategyUser, int normalRow, int finnedRow,
        IReadOnlyLinePositions normalPos, IReadOnlyLinePositions finnedPos, int number)
    {
        var asArray = normalPos.ToArray();

        if (finnedPos.Contains(asArray[0]) && HasSameMiniCol(finnedPos, asArray[1], asArray[0]))
            ProcessRow(strategyUser, normalRow, finnedRow, asArray[1], number);
        
        if (finnedPos.Contains(asArray[1]) && HasSameMiniCol(finnedPos, asArray[0], asArray[1]))
            ProcessRow(strategyUser, normalRow, finnedRow, asArray[0], number);

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(this,
            new FinnedXWingReportBuilder(normalPos, normalRow, finnedPos,
                finnedRow, number, Unit.Row)) && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private bool ExamineColumn(IStrategyUser strategyUser, int normalCol, int finnedCol,
        IReadOnlyLinePositions normalPos, IReadOnlyLinePositions finnedPos, int number)
    {
        var asArray = normalPos.ToArray();

        if (finnedPos.Contains(asArray[0]) && HasSameMiniRow(finnedPos, asArray[1], asArray[0]))
            ProcessColumn(strategyUser, normalCol, finnedCol, asArray[1], number);
        
        if (finnedPos.Contains(asArray[1]) && HasSameMiniRow(finnedPos, asArray[0], asArray[1]))
            ProcessColumn(strategyUser, normalCol, finnedCol, asArray[0], number);

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(this,
                new FinnedXWingReportBuilder(normalPos, normalCol,
                    finnedPos, finnedCol, number, Unit.Column)) && OnCommitBehavior == OnCommitBehavior.Return;
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

    private void ProcessRow(IStrategyUser strategyUser, int normalRow, int finnedRow, int finnedCol, int number)
    {
        var startRow = finnedRow / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            int row = startRow + i;
            if (row == normalRow || row == finnedRow) continue;
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, row, finnedCol);
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
    
    private void ProcessColumn(IStrategyUser strategyUser, int normalCol, int finnedCol, int finnedRow, int number)
    {
        var startCol = finnedCol / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            int col = startCol + i;
            if (col == finnedCol || col == normalCol) continue;
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, finnedRow, col);
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

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
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
                        _normal.Contains(finnedOther) ? ChangeColoration.CauseOffOne : ChangeColoration.CauseOffTwo);
                else
                    lighter.HighlightPossibility(_number, finnedOther, _finnedUnit,
                        _normal.Contains(finnedOther) ? ChangeColoration.CauseOffOne : ChangeColoration.CauseOffTwo);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}