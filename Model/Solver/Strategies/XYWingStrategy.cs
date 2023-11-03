using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class XYWingStrategy : AbstractStrategy
{
    public const string OfficialName = "XY-Wing";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public XYWingStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior) {}

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        var map = new PositionsMap(strategyManager, Only2Possibilities);
        
        for (int row = 0; row < 9; row++)
        {
            foreach (var col in map.Rows[row])
            {
                var firstPoss = strategyManager.PossibilitiesAt(row, col);
                foreach (var otherCol in map.Rows[row])
                {
                    if (col == otherCol) continue;
                    
                    var secondPoss = strategyManager.PossibilitiesAt(row, otherCol);
                    if(!firstPoss.PeekOnlyOne(secondPoss)) continue;

                    //Rows & Cols
                    foreach (var otherRow in map.Columns[col])
                    {
                        if(row == otherRow) continue;

                        var thirdPoss = strategyManager.PossibilitiesAt(otherRow, col);
                        var and = thirdPoss.And(secondPoss);
                        int toRemove;
                        if(and.Count != 1 || firstPoss.Peek(toRemove = and.First()) || !firstPoss.PeekOnlyOne(thirdPoss)) continue;

                        if(Process(strategyManager, row, col, row, otherCol, otherRow, col, toRemove))
                            return;
                    }

                    //Rows & Minis
                    foreach (var mini in map.Minis[row / 3, col / 3])
                    {
                        if(mini.Row == row) continue;

                        var thirdPoss = strategyManager.PossibilitiesAt(mini.Row, mini.Col);
                        var and = thirdPoss.And(secondPoss);
                        int toRemove;
                        if(and.Count != 1 || firstPoss.Peek(toRemove = and.First()) || !firstPoss.PeekOnlyOne(thirdPoss)) continue;

                        if (Process(strategyManager, row, col, row, otherCol, mini.Row, mini.Col, toRemove))
                            return;
                    }
                }
                
                //Cols & Minis
                foreach (var otherRow in map.Columns[col])
                {
                    if(row == otherRow) continue;

                    var secondPoss = strategyManager.PossibilitiesAt(otherRow, col);
                    if(!firstPoss.PeekOnlyOne(secondPoss)) continue;
                    
                    foreach (var mini in map.Minis[row / 3, col / 3])
                    {
                        if(mini.Col == col) continue;

                        var thirdPoss = strategyManager.PossibilitiesAt(mini.Row, mini.Col);
                        var and = thirdPoss.And(secondPoss);
                        int toRemove;
                        if(and.Count != 1 || firstPoss.Peek(toRemove = and.First()) || !firstPoss.PeekOnlyOne(thirdPoss)) continue;

                        if (Process(strategyManager, row, col, otherRow, col, mini.Row, mini.Col, toRemove))
                            return;
                    }
                }
            }
        }
    }

    private static bool Only2Possibilities(IReadOnlyPossibilities possibilities)
    {
        return possibilities.Count == 2;
    }

    private bool Process(IStrategyManager strategyManager, int hingeRow, int hingeCol,
        int row1, int col1, int row2, int col2, int number)
    {
        foreach (var cell in Cells.SharedSeenCells(row1, col1, row2, col2))
        {
            strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, cell.Row, cell.Col);
        }

        return strategyManager.ChangeBuffer.Commit(this,
            new XYWingReportBuilder(hingeRow, hingeCol, row1, col1, row2, col2))
            && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class XYWingReportBuilder : IChangeReportBuilder
{
    private readonly int _hingeRow;
    private readonly int _hingeCol;
    private readonly int _row1;
    private readonly int _col1;
    private readonly int _row2;
    private readonly int _col2;

    public XYWingReportBuilder(int hingeRow, int hingeCol, int row1, int col1, int row2, int col2)
    {
        _hingeRow = hingeRow;
        _hingeCol = hingeCol;
        _row1 = row1;
        _col1 = col1;
        _row2 = row2;
        _col2 = col2;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
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