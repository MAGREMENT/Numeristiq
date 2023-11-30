using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class TwoStringKiteStrategy : AbstractStrategy
{
    public const string OfficialName = "Two-String Kite";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public TwoStringKiteStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var rowPositions = strategyManager.RowPositionsAt(row, number);
                if (rowPositions.Count != 2 || rowPositions.AreAllInSameMiniGrid()) continue;

                for (int col = 0; col < 9; col++)
                {
                    var colPositions = strategyManager.ColumnPositionsAt(col, number);
                    if (colPositions.Count != 2 || colPositions.AreAllInSameMiniGrid()) continue;

                    var rowCell = rowPositions.ToCellArray(Unit.Row, row);
                    var colCell = colPositions.ToCellArray(Unit.Column, col);

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            var rCommon = rowCell[i];
                            var cCommon = colCell[j];
                            if (rCommon == cCommon) continue;
                            if(rCommon.Row / 3 != cCommon.Row / 3 || rCommon.Column / 3 != cCommon.Column / 3) continue;

                            var rOther = rowCell[(i + 1) % 2];
                            var cOther = colCell[(j + 1) % 2];

                            foreach (var c in Cells.SharedSeenCells(rOther, cOther))
                            {
                                strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, c);
                            }

                            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                                    new TwoStringKiteReportBuilder(number, rCommon, cCommon, rOther,
                                        cOther)) && OnCommitBehavior == OnCommitBehavior.Return) return;
                        }
                    }
                }
            }
        }
    }
}

public class TwoStringKiteReportBuilder : IChangeReportBuilder
{
    private readonly int _possibility;
    private readonly Cell _inCommon1;
    private readonly Cell _inCommon2;
    private readonly Cell _other1;
    private readonly Cell _other2;

    public TwoStringKiteReportBuilder(int possibility, Cell inCommon1, Cell inCommon2, Cell other1, Cell other2)
    {
        _possibility = possibility;
        _inCommon1 = inCommon1;
        _inCommon2 = inCommon2;
        _other1 = other1;
        _other2 = other2;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightPossibility(_possibility, _inCommon1.Row, _inCommon1.Column, ChangeColoration.CauseOffOne);
            lighter.HighlightPossibility(_possibility, _inCommon2.Row, _inCommon2.Column, ChangeColoration.CauseOffOne);
            lighter.HighlightPossibility(_possibility, _other1.Row, _other1.Column, ChangeColoration.CauseOffOne);
            lighter.HighlightPossibility(_possibility, _other2.Row, _other2.Column, ChangeColoration.CauseOffOne);
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}