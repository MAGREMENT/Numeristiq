using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.SudokuSolving.Solver.Helpers.Changes;
using Model.SudokuSolving.Solver.Possibility;
using Model.SudokuSolving.Solver.StrategiesUtility;

namespace Model.SudokuSolving.Solver.Strategies;

public class XYZWingStrategy : AbstractStrategy
{
    public const string OfficialName = "XYZ-Wing";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public XYZWingStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior) {}

    public override void Apply(IStrategyManager strategyManager)
    {
        var map = new PositionsMap(strategyManager, Only2Possibilities);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var hinge = strategyManager.PossibilitiesAt(row, col);
                if(hinge.Count != 3) continue;

                var miniRow = row / 3;
                var miniCol = col / 3;
                foreach (var mini in map.Minis[row / 3, col / 3])
                {
                    if (mini.Row == row && mini.Column == col) continue;

                    var firstCorner = strategyManager.PossibilitiesAt(mini.Row, mini.Column);
                    Possibilities and;
                    if ((and = firstCorner.And(hinge)).Count != 2) continue;

                    foreach (var otherCol in map.Rows[row])
                    {
                        if(otherCol / 3 == miniCol) continue;

                        var secondCorner = strategyManager.PossibilitiesAt(row, otherCol);
                        if(!secondCorner.Or(firstCorner).Equals(hinge)) continue;

                        if (Process(strategyManager, row, col, mini.Row, mini.Column, row,
                                otherCol, and.And(secondCorner).First())) return;
                    }

                    foreach (var otherRow in map.Columns[col])
                    {
                        if(otherRow / 3 == miniRow) continue;

                        var secondCorner = strategyManager.PossibilitiesAt(otherRow, col);
                        if(!secondCorner.Or(firstCorner).Equals(hinge)) continue;

                        if (Process(strategyManager, row, col, mini.Row, mini.Column,
                                otherRow, col, and.And(secondCorner).First())) return;
                    }
                }
            }
        }
    }

    private bool Process(IStrategyManager strategyManager, int hingeRow, int hingeCol, int row1, int col1, int row2,
        int col2, int number)
    {
        foreach (var cell in Cells.SharedSeenCells(new Cell(hingeRow, hingeCol),
                     new Cell(row1, col1), new Cell(row2, col2)))
        {
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, cell.Row, cell.Column);
        }

        return strategyManager.ChangeBuffer.Commit(this,
            new XYZWingReportBuilder(hingeRow, hingeCol, row1, col1, row2, col2)) && OnCommitBehavior == OnCommitBehavior.Return;
    }
    
    private static bool Only2Possibilities(IReadOnlyPossibilities possibilities)
    {
        return possibilities.Count == 2;
    }
}

public class XYZWingReportBuilder : IChangeReportBuilder
{
    private readonly int _hingeRow;
    private readonly int _hingeCol;
    private readonly int _row1;
    private readonly int _col1;
    private readonly int _row2;
    private readonly int _col2;

    public XYZWingReportBuilder(int hingeRow, int hingeCol, int row1, int col1, int row2, int col2)
    {
        _hingeRow = hingeRow;
        _hingeCol = hingeCol;
        _row1 = row1;
        _col1 = col1;
        _row2 = row2;
        _col2 = col2;
    }
    
    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_hingeRow, _hingeCol, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_row1, _col1, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_row2, _col2, ChangeColoration.CauseOffOne);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}