using System.Collections.Generic;
using Model.Positions;
using Model.StrategiesUtil;

namespace Model.Strategies;

/// <summary>
/// This class generalize the XWing, Swordfish and JellyFish strategy
/// </summary>
public class GridFormationStrategy : IStrategy
{
    public string Name { get; }

    public StrategyLevel Difficulty { get; }
    public int Score { get; set; }

    private readonly int _type;

    public GridFormationStrategy(int type)
    {
        _type = type;
        switch (type)
        {
            case 2 : Name = "XWing";
                Difficulty = StrategyLevel.Hard;
                break;
            case 3 : Name = "Swordfish";
                Difficulty = StrategyLevel.Hard;
                break;
            case 4 : Name = "Jellyfish";
                Difficulty = StrategyLevel.Hard;
                break;
            default : Name = "Grid formation unknown";
                Difficulty = StrategyLevel.None;
                break;
        }
    }

    public void ApplyOnce(IStrategyManager strategyManager)
    { 
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                Search(strategyManager, row, number, new LinePositions(), new LinePositions(), Unit.Row);
            }
            
            for (int col = 0; col < 9; col++)
            {
                Search(strategyManager, col, number, new LinePositions(), new LinePositions(), Unit.Column);
            }
        }
    }

    private void Search(IStrategyManager strategyManager, int unitToSearch, int number, LinePositions mashed, LinePositions visited,
        Unit unit)
    {
        visited.Add(unitToSearch);
        
        var current = unit == Unit.Row
            ? strategyManager.PossibilityPositionsInRow(unitToSearch, number)
            : strategyManager.PossibilityPositionsInColumn(unitToSearch, number);
        if (current.Count > _type || current.Count < 2) return;

        var newMashed = mashed.Mash(current);
        if (newMashed.Count > _type) return;

        if (visited.Count == _type)
        {
            if(newMashed.Count == _type)Process(strategyManager, visited, newMashed, number, unit);
        }
        else
        {
            for (int i = unitToSearch + 1; i < 9; i++)
            {
                Search(strategyManager, i, number, newMashed, visited.Copy(), unit);
            }
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

        strategyManager.ChangeBuffer.Push(this,
            unit == Unit.Row
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

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        List<Coordinate> coords = new();
        foreach (var row in _rows)
        {
            foreach (var col in _cols)
            {
                if (manager.Possibilities[row, col].Peek(_number)) coords.Add(new Coordinate(row, col));
            }
        }
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(_number, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        }, "");
    }
}