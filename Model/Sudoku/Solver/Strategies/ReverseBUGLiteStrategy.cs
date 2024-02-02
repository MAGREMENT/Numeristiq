using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.Possibility;

namespace Model.Sudoku.Solver.Strategies;

public class ReverseBUGLiteStrategy : AbstractStrategy
{
    public const string OfficialName = "Reverse BUG-Lite";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public ReverseBUGLiteStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int row1 = 0; row1 < 9; row1++)
        {
            var startR = row1 / 3 * 3;
            var miniR = row1 % 3;
            
            for (int r = miniR + 1; r < 3; r++)
            {
                var row2 = startR + r;
                
                var poss1 = Possibilities.NewEmpty();
                var poss2 = Possibilities.NewEmpty();

                var cols1 = new LinePositions();
                var cols2 = new LinePositions();

                for (int col = 0; col < 9; col++)
                {
                    var current = strategyManager.Sudoku[row1, col];
                    if (current != 0)
                    {
                        poss1.Add(current);
                        cols1.Add(col);
                    }
                    
                    current = strategyManager.Sudoku[row2, col];
                    if (current != 0)
                    {
                        poss2.Add(current);
                        cols2.Add(col);
                    }
                }

                var and = poss1.And(poss2);
                var solo = poss1.Difference(and).Or(poss2).Difference(and);
                if (solo.Count != 1) continue;

                var or = cols1.Or(cols2);
                if (or.Count != Math.Max(cols1.Count, cols2.Count)) continue;

                var p = solo.First();
                foreach (var col in or)
                {
                    if (!cols1.Peek(col))
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, row1, col);
                        break;
                    }

                    if (!cols2.Peek(col))
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, row2, col);
                        break;
                    }
                }

                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                        new ReverseBUGLiteChangeReport(row1, row2, cols1, cols2, Unit.Row)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
        
        for (int col1 = 0; col1 < 9; col1++)
        {
            var startC = col1 / 3 * 3;
            var miniC = col1 % 3;
            
            for (int c = miniC + 1; c < 3; c++)
            {
                var col2 = startC + c;
                
                var poss1 = Possibilities.NewEmpty();
                var poss2 = Possibilities.NewEmpty();

                var rows1 = new LinePositions();
                var rows2 = new LinePositions();

                for (int row = 0; row < 9; row++)
                {
                    var current = strategyManager.Sudoku[row, col1];
                    if (current != 0)
                    {
                        poss1.Add(current);
                        rows1.Add(row);
                    }
                    
                    current = strategyManager.Sudoku[row, col2];
                    if (current != 0)
                    {
                        poss2.Add(current);
                        rows2.Add(row);
                    }
                }

                var and = poss1.And(poss2);
                var solo = poss1.Difference(and).Or(poss2).Difference(and);
                if (solo.Count != 1) continue;

                var or = rows1.Or(rows2);
                if (or.Count != Math.Max(rows1.Count, rows2.Count)) continue;

                var p = solo.First();
                foreach (var row in or)
                {
                    if (!rows1.Peek(row))
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, row, col1);
                        break;
                    }

                    if (!rows2.Peek(row))
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, row, col2);
                        break;
                    }
                }

                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                        new ReverseBUGLiteChangeReport(col1, col2, rows1, rows2, Unit.Column)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
    }
}

public class ReverseBUGLiteChangeReport : IChangeReportBuilder
{
    private readonly int _unit1;
    private readonly int _unit2;
    private readonly LinePositions _others1;
    private readonly LinePositions _others2;
    private readonly Unit _unit;

    public ReverseBUGLiteChangeReport(int unit1, int unit2, LinePositions others1, LinePositions others2, Unit unit)
    {
        _unit1 = unit1;
        _unit2 = unit2;
        _others1 = others1;
        _others2 = others2;
        _unit = unit;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var o in _others1)
            {
                if (_unit == Unit.Row) lighter.HighlightCell(_unit1, o, ChangeColoration.CauseOffOne);
                else lighter.HighlightCell(o, _unit1, ChangeColoration.CauseOffOne);
            }
            
            foreach (var o in _others2)
            {
                if (_unit == Unit.Row) lighter.HighlightCell(_unit2, o, ChangeColoration.CauseOffOne);
                else lighter.HighlightCell(o, _unit2, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}