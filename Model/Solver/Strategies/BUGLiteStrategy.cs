using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.Possibility;

namespace Model.Solver.Strategies;

public class BUGLiteStrategy : AbstractStrategy
{
    public const string OfficialName = "BUG-Lite";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly int _maxStructSize;
    
    public BUGLiteStrategy(int maxStructSize) : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        _maxStructSize = maxStructSize;
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(IStrategyManager strategyManager) //TODO correct by adding single possibility condition
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var poss = strategyManager.PossibilitiesAt(row, col);
                if (poss.Count != 2) continue;

                var first = new Cell(row, col);
                var startR = row / 3 * 3;
                var startC = col / 3 * 3;

                for (int r = row % 3; r < 3; r++)
                {
                    var row2 = startR + r;
                    
                    for (int c = col % 3; c < 3; c++)
                    {
                        var col2 = startC + c;
                        if ((row2 == row && col2 == col) || !strategyManager.PossibilitiesAt(row2, col2).Equals(poss)) continue;

                        var second = new Cell(row2, col2);
                        var bcp = new BiCellPossibilities(first, second, poss);
                        var conditions = new List<IBUGLiteCondition>();
                        if (row != row2) conditions.Add(new RowBUGLiteCondition(bcp));
                        if (col != col2) conditions.Add(new ColumnBUGLiteCondition(bcp));
                        
                        if (Search(strategyManager, new List<BiCellPossibilities> {bcp},
                            new GridPositions {first, second}, conditions)) return;
                    }
                }
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, List<BiCellPossibilities> bcp, GridPositions done, List<IBUGLiteCondition> conditions)
    {
        var current = conditions[0];
        conditions.RemoveAt(0);

        foreach (var match in current.ConditionMatches(strategyManager, done))
        {
            done.Add(match.BiCellPossibilities.One);
            done.Add(match.BiCellPossibilities.Two);
            conditions.AddRange(match.OtherConditions);
            bcp.Add(match.BiCellPossibilities);

            if (conditions.Count == 0)
            {
                if(Process(strategyManager, bcp)) return true;
            }
            else if (done.Count < _maxStructSize && Search(strategyManager, bcp, done, conditions)) return true;

            done.Remove(match.BiCellPossibilities.One);
            done.Remove(match.BiCellPossibilities.Two);
            conditions.RemoveRange(conditions.Count - match.OtherConditions.Length, match.OtherConditions.Length);
            bcp.RemoveAt(bcp.Count - 1);
        }
        
        return false;
    }

    private bool Process(IStrategyManager strategyManager, List<BiCellPossibilities> bcp)
    {
        var notInStructure = new List<Cell>();

        foreach (var b in bcp)
        {
            if(strategyManager.PossibilitiesAt(b.One).Difference(b.Possibilities).Count > 0) notInStructure.Add(b.One);

            if(strategyManager.PossibilitiesAt(b.Two).Difference(b.Possibilities).Count > 0) notInStructure.Add(b.Two);
        }

        if (notInStructure.Count == 1)
        {
            var c = notInStructure[0];
            foreach (var p in FindStructurePossibilitiesFor(c, bcp))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, c);
            }
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new BUGLiteReportBuilder(bcp)) && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private static IReadOnlyPossibilities FindStructurePossibilitiesFor(Cell cell, List<BiCellPossibilities> bcp)
    {
        foreach (var b in bcp)
        {
            if (b.One == cell || b.Two == cell) return b.Possibilities;
        }

        return Possibilities.NewEmpty();
    }
}

public record BiCellPossibilities(Cell One, Cell Two, IReadOnlyPossibilities Possibilities);

public record BUGLiteConditionMatch(BiCellPossibilities BiCellPossibilities, params IBUGLiteCondition[] OtherConditions);

public interface IBUGLiteCondition
{ 
    IEnumerable<BUGLiteConditionMatch> ConditionMatches(IStrategyManager strategyManager, GridPositions done);
}

public class RowBUGLiteCondition : IBUGLiteCondition
{
    private readonly BiCellPossibilities _bcp;

    public RowBUGLiteCondition(BiCellPossibilities bcp)
    {
        _bcp = bcp;
    }

    public IEnumerable<BUGLiteConditionMatch> ConditionMatches(IStrategyManager strategyManager, GridPositions done)
    {
        var miniRow = _bcp.One.Row / 3;

        for (int r = 0; r < 3; r++)
        {
            if (r == miniRow) yield break;

            for (int i = 0; i < 3; i++)
            {
                var first = new Cell(r * 3 + i, _bcp.One.Column);
                if (done.Peek(first) || strategyManager.Sudoku[first.Row, first.Column] != 0) continue;

                for (int j = 0; j < 3; j++)
                {
                    var second = new Cell(r * 3 + j, _bcp.Two.Column);
                    if (done.Peek(second) || strategyManager.Sudoku[second.Row, second.Column] != 0) continue;

                    var and = strategyManager.PossibilitiesAt(first).And(strategyManager.PossibilitiesAt(second));
                    if (and.Equals(_bcp.Possibilities))
                    {
                        var otherBcp = new BiCellPossibilities(first, second, and);
                        if (i == j) yield return new BUGLiteConditionMatch(otherBcp);
                        else yield return new BUGLiteConditionMatch(otherBcp, new ColumnBUGLiteCondition(otherBcp));
                    }
                    else
                    {
                        foreach (var current in _bcp.Possibilities)
                        {
                            if (!and.Peek(current)) continue;

                            foreach (var otherPoss in and)
                            {
                                if (otherPoss == current) continue;

                                var newPoss = Possibilities.NewEmpty();
                                newPoss.Add(current);
                                newPoss.Add(otherPoss);
                                var otherBcp = new BiCellPossibilities(first, second, newPoss);

                                if (i == j) yield return new BUGLiteConditionMatch(otherBcp, new RowBUGLiteCondition(otherBcp));
                                else yield return new BUGLiteConditionMatch(otherBcp, new RowBUGLiteCondition(otherBcp),
                                    new ColumnBUGLiteCondition(otherBcp));
                            }
                        }
                    }
                }
            }
        }
    }
}

public class ColumnBUGLiteCondition : IBUGLiteCondition
{
    private readonly BiCellPossibilities _bcp;

    public ColumnBUGLiteCondition(BiCellPossibilities bcp)
    {
        _bcp = bcp;
    }

    public IEnumerable<BUGLiteConditionMatch> ConditionMatches(IStrategyManager strategyManager, GridPositions done)
    {
        var miniCol = _bcp.One.Column / 3;

        for (int c = 0; c < 3; c++)
        {
            if (c == miniCol) yield break;

            for (int i = 0; i < 3; i++)
            {
                var first = new Cell(_bcp.One.Row, c * 3 + i);
                if (done.Peek(first) || strategyManager.Sudoku[first.Row, first.Column] != 0) continue;

                for (int j = 0; j < 3; j++)
                {
                    var second = new Cell(_bcp.Two.Row, c * 3 + j);
                    if (done.Peek(second) || strategyManager.Sudoku[second.Row, second.Column] != 0) continue;

                    var and = strategyManager.PossibilitiesAt(first).And(strategyManager.PossibilitiesAt(second));
                    if (and.Equals(_bcp.Possibilities))
                    {
                        var otherBcp = new BiCellPossibilities(first, second, and);
                        if (i == j) yield return new BUGLiteConditionMatch(otherBcp);
                        else yield return new BUGLiteConditionMatch(otherBcp, new RowBUGLiteCondition(otherBcp));
                    }
                    else
                    {
                        foreach (var current in _bcp.Possibilities)
                        {
                            if (!and.Peek(current)) continue;

                            foreach (var otherPoss in and)
                            {
                                if (otherPoss == current) continue;

                                var newPoss = Possibilities.NewEmpty();
                                newPoss.Add(current);
                                newPoss.Add(otherPoss);
                                var otherBcp = new BiCellPossibilities(first, second, newPoss);

                                if (i == j) yield return new BUGLiteConditionMatch(otherBcp, new ColumnBUGLiteCondition(otherBcp));
                                else yield return new BUGLiteConditionMatch(otherBcp, new ColumnBUGLiteCondition(otherBcp),
                                    new RowBUGLiteCondition(otherBcp));
                            }
                        }
                    }
                }
            }
        }
    }
}

public static class BUGLiteHelper {
    
    public static IEnumerable<BiCell> PossibleRowMatches(IStrategyManager strategyManager, Cell one, Cell two)
    {
        var miniRow = one.Row / 3;

        for (int r = 0; r < 3; r++)
        {
            if (r == miniRow) continue;

            for (int i = 0; i < 3; i++)
            {
                var first = new Cell(r * 3 + i, one.Column);
                if (strategyManager.Sudoku[first.Row, first.Column] != 0) continue;

                for (int j = 0; j < 3; j++)
                {
                    var second = new Cell(r * 3 + j, two.Column);
                    if (strategyManager.Sudoku[second.Row, second.Column] != 0) continue;

                    yield return new BiCell(first, second);
                }
            }
        }
    }
    
    public static IEnumerable<BiCell> PossibleColumnMatches(IStrategyManager strategyManager, Cell one, Cell two)
    {
        var miniCol = one.Column / 3;

        for (int c = 0; c < 3; c++)
        {
            if (c == miniCol) continue;

            for (int i = 0; i < 3; i++)
            {
                var first = new Cell(one.Row, c * 3 + i);
                if (strategyManager.Sudoku[first.Row, first.Column] != 0) continue;

                for (int j = 0; j < 3; j++)
                {
                    var second = new Cell(two.Row, c * 3 + j);
                    if (strategyManager.Sudoku[second.Row, second.Column] != 0) continue;

                    yield return new BiCell(first, second);
                }
            }
        }
    }
}

public readonly struct BiCell
{
    public BiCell(Cell one, Cell two)
    {
        One = one;
        Two = two;
    }

    public Cell One { get; }
    public Cell Two { get; }
}

public class BUGLiteReportBuilder : IChangeReportBuilder
{
    private readonly List<BiCellPossibilities> _bcp;

    public BUGLiteReportBuilder(List<BiCellPossibilities> bcp)
    {
        _bcp = bcp;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var b in _bcp)
            {
                foreach (var p in b.Possibilities)
                {
                    lighter.HighlightPossibility(p, b.One.Row, b.One.Column, ChangeColoration.CauseOffTwo);
                    lighter.HighlightPossibility(p, b.Two.Row, b.Two.Column, ChangeColoration.CauseOffTwo);
                }
            }
        });
    }
}