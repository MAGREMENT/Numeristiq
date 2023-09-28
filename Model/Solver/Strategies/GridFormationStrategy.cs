using System;
using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.StrategiesUtil;
using Model.StrategiesUtil;

namespace Model.Solver.Strategies;

/// <summary>
/// This class generalize the XWing, Swordfish and JellyFish strategy
/// </summary>
public class GridFormationStrategy : AbstractStrategy
{
    public const string OfficialNameForType2 = "X-Wing";
    public const string OfficialNameForType3 = "Swordfish";
    public const string OfficialNameForType4 = "Jellyfish";

    private readonly int _type;

    public GridFormationStrategy(int type) : base("", StrategyDifficulty.None)
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

    public override void ApplyOnce(IStrategyManager strategyManager)
    { 
        for (int number = 1; number <= 9; number++)
        {
            Search(strategyManager, 0, Unit.Row, number, new LinePositions(), new LinePositions());
            Search(strategyManager, 0, Unit.Column, number, new LinePositions(), new LinePositions());
        }
    }

    private void Search(IStrategyManager strategyManager, int start, Unit unit, int number, LinePositions or,
        LinePositions visited)
    {
        for (int i = start; i < 9; i++)
        {
            var current = unit == Unit.Row
                ? strategyManager.RowPositionsAt(i, number)
                : strategyManager.ColumnPositionsAt(i, number);
            if (current.Count > _type || current.Count < 1) continue;

            var newOr = or.Or(current);
            if(newOr.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type)
            {
                if (newOr.Count == _type) Process(strategyManager, newVisited, newOr, number, unit);
            }
            else Search(strategyManager, i + 1, unit, number, newOr, newVisited);
        }
    }

    private void Process(IStrategyManager strategyManager, LinePositions visited, LinePositions toRemove, int number, Unit unit)
    {
        foreach (var first in toRemove)
        {
            for (int other = 0; other < 9; other++)
            {
                if (visited.Peek(other)) continue;

                if (unit == Unit.Row) strategyManager.ChangeBuffer.AddPossibilityToRemove(number, other, first);
                else strategyManager.ChangeBuffer.AddPossibilityToRemove(number, first, other);
            }
        }

        strategyManager.ChangeBuffer.Push(this, unit == Unit.Row
                ? new GridFormationReportBuilder(visited, toRemove, number)
                : new GridFormationReportBuilder(toRemove, visited, number));
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Cell> coords = new();
        foreach (var row in _rows)
        {
            foreach (var col in _cols)
            {
                if (snapshot.PossibilitiesAt(row, col).Peek(_number)) coords.Add(new Cell(row, col));
            }
        }
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(_number, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}