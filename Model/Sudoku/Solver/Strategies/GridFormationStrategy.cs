using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.Position;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

/// <summary>
/// This class generalize the XWing, Swordfish and JellyFish strategy
/// </summary>
public class GridFormationStrategy : SudokuStrategy
{
    public const string OfficialNameForType2 = "X-Wing";
    public const string OfficialNameForType3 = "Swordfish";
    public const string OfficialNameForType4 = "Jellyfish";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly int _type;

    public GridFormationStrategy(int type) : base("", StrategyDifficulty.None, DefaultBehavior)
    {
        _type = type;
        switch (type)
        {
            case 2 : Name =  OfficialNameForType2;
                Difficulty = StrategyDifficulty.Medium;
                break;
            case 3 : Name = OfficialNameForType3;
                Difficulty = StrategyDifficulty.Medium;
                break;
            case 4 : Name = OfficialNameForType4;
                Difficulty = StrategyDifficulty.Medium;
                break;
            default : throw new ArgumentException("Type not valid");
        }
    }

    public override void Apply(IStrategyUser strategyUser)
    { 
        for (int number = 1; number <= 9; number++)
        {
            if (Search(strategyUser, 0, Unit.Row, number, new LinePositions(), new LinePositions())) return;
            if (Search(strategyUser, 0, Unit.Column, number, new LinePositions(), new LinePositions())) return;
        }
    }

    private bool Search(IStrategyUser strategyUser, int start, Unit unit, int number, LinePositions or,
        LinePositions visited)
    {
        for (int i = start; i < 9; i++)
        {
            var current = unit == Unit.Row
                ? strategyUser.RowPositionsAt(i, number)
                : strategyUser.ColumnPositionsAt(i, number);
            if (current.Count > _type || current.Count < 1) continue;

            var newOr = or.Or(current);
            if(newOr.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type)
            {
                if (newOr.Count == _type && Process(strategyUser, newVisited, newOr, number, unit)) return true;
            }
            else Search(strategyUser, i + 1, unit, number, newOr, newVisited);
        }

        return false;
    }

    private bool Process(IStrategyUser strategyUser, LinePositions visited, LinePositions toRemove, int number, Unit unit)
    {
        foreach (var first in toRemove)
        {
            for (int other = 0; other < 9; other++)
            {
                if (visited.Contains(other)) continue;

                if (unit == Unit.Row) strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, other, first);
                else strategyUser.ChangeBuffer.ProposePossibilityRemoval(number, first, other);
            }
        }

        return strategyUser.ChangeBuffer.Commit( unit == Unit.Row
                ? new GridFormationReportBuilder(visited, toRemove, number)
                : new GridFormationReportBuilder(toRemove, visited, number)) 
               && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class GridFormationReportBuilder : IChangeReportBuilder
{
    private readonly LinePositions _rows;
    private readonly LinePositions _cols;
    private readonly int _number;

    public GridFormationReportBuilder(LinePositions rows, LinePositions cols, int number)
    {
        _rows = rows;
        _cols = cols;
        _number = number;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, ISudokuSolvingState snapshot)
    {
        List<Cell> coords = new();
        foreach (var row in _rows)
        {
            foreach (var col in _cols)
            {
                if (snapshot.PossibilitiesAt(row, col).Contains(_number)) coords.Add(new Cell(row, col));
            }
        }
        return new ChangeReport( "", lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(_number, coord.Row, coord.Column, ChangeColoration.CauseOffOne);
            }
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}