using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Sudoku.Solver.Strategies;

public class XYWingStrategy : AbstractStrategy
{
    public const string OfficialName = "XY-Wing";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public XYWingStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior) {}

    public override void Apply(IStrategyUser strategyUser)
    {
        var map = new PositionsMap(strategyUser, Only2Possibilities);
        
        for (int row = 0; row < 9; row++)
        {
            foreach (var col in map.Rows[row])
            {
                var firstPoss = strategyUser.PossibilitiesAt(row, col);
                foreach (var otherCol in map.Rows[row])
                {
                    if (col == otherCol) continue;
                    
                    var secondPoss = strategyUser.PossibilitiesAt(row, otherCol);
                    if(!firstPoss.ContainsOnlyOne(secondPoss)) continue;

                    //Rows & Cols
                    foreach (var otherRow in map.Columns[col])
                    {
                        if(row == otherRow) continue;

                        var thirdPoss = strategyUser.PossibilitiesAt(otherRow, col);
                        var and = thirdPoss & secondPoss;
                        int toRemove;
                        if(and.Count != 1 || firstPoss.Contains(toRemove = and.FirstPossibility()) || !firstPoss.ContainsOnlyOne(thirdPoss)) continue;

                        if(Process(strategyUser, row, col, row, otherCol, otherRow, col, toRemove))
                            return;
                    }

                    //Rows & Minis
                    foreach (var mini in map.Minis[row / 3, col / 3])
                    {
                        if(mini.Row == row) continue;

                        var thirdPoss = strategyUser.PossibilitiesAt(mini.Row, mini.Column);
                        var and = thirdPoss & secondPoss;
                        int toRemove;
                        if(and.Count != 1 || firstPoss.Contains(toRemove = and.FirstPossibility()) || !firstPoss.ContainsOnlyOne(thirdPoss)) continue;

                        if (Process(strategyUser, row, col, row, otherCol, mini.Row, mini.Column, toRemove))
                            return;
                    }
                }
                
                //Cols & Minis
                foreach (var otherRow in map.Columns[col])
                {
                    if(row == otherRow) continue;

                    var secondPoss = strategyUser.PossibilitiesAt(otherRow, col);
                    if(!firstPoss.ContainsOnlyOne(secondPoss)) continue;
                    
                    foreach (var mini in map.Minis[row / 3, col / 3])
                    {
                        if(mini.Column == col) continue;

                        var thirdPoss = strategyUser.PossibilitiesAt(mini.Row, mini.Column);
                        var and = thirdPoss & secondPoss;
                        int toRemove;
                        if(and.Count != 1 || firstPoss.Contains(toRemove = and.FirstPossibility()) || !firstPoss.ContainsOnlyOne(thirdPoss)) continue;

                        if (Process(strategyUser, row, col, otherRow, col, mini.Row, mini.Column, toRemove))
                            return;
                    }
                }
            }
        }
    }

    private static bool Only2Possibilities(ReadOnlyBitSet16 possibilities)
    {
        return possibilities.Count == 2;
    }

    private bool Process(IStrategyUser strategyUser, int hingeRow, int hingeCol,
        int row1, int col1, int row2, int col2, int number)
    {
        foreach (var cell in Cells.SharedSeenCells(row1, col1, row2, col2))
        {
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, cell.Row, cell.Column);
        }

        return strategyUser.ChangeBuffer.Commit(
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
    
    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            lighter.HighlightCell(_hingeRow, _hingeCol, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_row1, _col1, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_row2, _col2, ChangeColoration.CauseOffOne);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}