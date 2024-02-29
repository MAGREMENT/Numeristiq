using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.Position;

namespace Model.Sudoku.Solver.Strategies;

public class SkyscraperStrategy : SudokuStrategy
{
    public const string OfficialName = "Skyscraper";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public SkyscraperStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
    }

    
    public override void Apply(IStrategyUser strategyUser)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row1 = 0; row1 < 8; row1++)
            {
                var pos1 = strategyUser.RowPositionsAt(row1, number);
                if (pos1.Count != 2) continue;

                for (int row2 = row1 + 1; row2 < 9; row2++)
                {
                    var pos2 = strategyUser.RowPositionsAt(row2, number);
                    if (pos2.Count != 2) continue;

                    var and = pos1.And(pos2);
                    if (and.Count != 1) continue;

                    var common = and.First();
                    var other1 = pos1.First(common);
                    var other2 = pos2.First(common);
                    if (other1 / 3 != other2 / 3) continue;

                    var startRow1 = row1 / 3 * 3;
                    var startRow2 = row2 / 3 * 3;

                    for (int r = 0; r < 3; r++)
                    {
                        var r1 = startRow1 + r;
                        var r2 = startRow2 + r;
                        
                        if(r1 != row2) strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, r1, other2);
                        if(r2 != row1) strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, r2, other1);
                    }

                    if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                            new SkyscraperReportBuilder(Unit.Row, row1, row2, pos1, pos2, number))
                                && OnCommitBehavior == OnCommitBehavior.Return) return;
                }
            }
            
            for (int col1 = 0; col1 < 8; col1++)
            {
                var pos1 = strategyUser.ColumnPositionsAt(col1, number);
                if (pos1.Count != 2) continue;

                for (int col2 = col1 + 1; col2 < 9; col2++)
                {
                    var pos2 = strategyUser.ColumnPositionsAt(col2, number);
                    if (pos2.Count != 2) continue;

                    var and = pos1.And(pos2);
                    if (and.Count != 1) continue;

                    var common = and.First();
                    var other1 = pos1.First(common);
                    var other2 = pos2.First(common);
                    if (other1 / 3 != other2 / 3) continue;

                    var startCol1 = col1 / 3 * 3;
                    var startCol2 = col2 / 3 * 3;

                    for (int c = 0; c < 3; c++)
                    {
                        var c1 = startCol1 + c;
                        var c2 = startCol2 + c;
                        
                        if(c1 != col2) strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, other2, c1);
                        if(c2 != col1) strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, other1, c2);
                    }

                    if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                            new SkyscraperReportBuilder(Unit.Column, col1, col2, pos1, pos2, number)) &&
                                OnCommitBehavior == OnCommitBehavior.Return) return;
                }
            }
        }
    }
}

public class SkyscraperReportBuilder : IChangeReportBuilder
{
    private readonly Unit _unit;
    private readonly int _unit1;
    private readonly int _unit2;
    private readonly IReadOnlyLinePositions _pos1;
    private readonly IReadOnlyLinePositions _pos2;
    private readonly int _possibility;

    public SkyscraperReportBuilder(Unit unit, int unit1, int unit2, IReadOnlyLinePositions pos1,
        IReadOnlyLinePositions pos2, int possibility)
    {
        _unit = unit;
        _unit1 = unit1;
        _unit2 = unit2;
        _pos1 = pos1;
        _pos2 = pos2;
        _possibility = possibility;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var cell in _pos1.ToCellArray(_unit, _unit1))
            {
                lighter.HighlightPossibility(_possibility, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
            }
            
            foreach (var cell in _pos2.ToCellArray(_unit, _unit2))
            {
                lighter.HighlightPossibility(_possibility, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}