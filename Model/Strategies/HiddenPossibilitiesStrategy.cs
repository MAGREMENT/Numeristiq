using System.Collections.Generic;
using Model.Changes;
using Model.Positions;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class HiddenPossibilitiesStrategy : IStrategy
{
    public string Name { get; }

    public StrategyLevel Difficulty { get; }
    public StatisticsTracker Tracker { get; } = new();

    private readonly int _type;

    public HiddenPossibilitiesStrategy(int type)
    {
        _type = type;
        switch (type)
        {
            case 2 : Name = "Hidden double";
                Difficulty = StrategyLevel.Easy;
                break;
            case 3 : Name = "Hidden triple";
                Difficulty = StrategyLevel.Easy;
                break;
            case 4 : Name = "Hidden quad";
                Difficulty = StrategyLevel.Easy;
                break;
            default : Name = "Hidden unknown";
                Difficulty = StrategyLevel.None;
                break;
        }
    }
    
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = strategyManager.RowPositions(row, number);
                if (pos.Count > _type || pos.Count == 0) continue;
                var possibilities = IPossibilities.NewEmpty();
                possibilities.Add(number);

                RecursiveRowMashing(strategyManager, number + 1, pos, possibilities, row);
            }
        }

        for (int col = 0; col < 9; col++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = strategyManager.ColumnPositions(col, number);
                if (pos.Count > _type || pos.Count == 0) continue;
                var possibilities = IPossibilities.NewEmpty();
                possibilities.Add(number);
                
                RecursiveColumnMashing(strategyManager, number + 1, pos, possibilities, col);
            }
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var pos = strategyManager.MiniGridPositions(miniRow, miniCol, number);
                    if (pos.Count > _type || pos.Count == 0) continue;
                    var possibilities = IPossibilities.NewEmpty();
                    possibilities.Add(number);
                    
                    RecursiveMiniGridMashing(strategyManager, number + 1, pos, possibilities, miniRow, miniCol);
                }
            }
        }
    }

    private void RecursiveRowMashing(IStrategyManager strategyManager, int start, LinePositions mashed,
        IPossibilities visited, int row)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = strategyManager.RowPositions(row, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Mash(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var col in newMashed)
                {
                    RemoveAllPossibilitiesExcept(row, col, newVisited, strategyManager.ChangeBuffer);
                }

                strategyManager.ChangeBuffer.Push(this,
                    new LineHiddenPossibilitiesReportBuilder(newVisited, newMashed, row, Unit.Row));
            }
            else if (newVisited.Count < _type) RecursiveRowMashing(strategyManager, i + 1, newMashed, newVisited, row);
        }
    }
    
    private void RecursiveColumnMashing(IStrategyManager strategyManager, int start, LinePositions mashed,
        IPossibilities visited, int col)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = strategyManager.ColumnPositions(col, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Mash(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var row in newMashed)
                {
                    RemoveAllPossibilitiesExcept(row, col, newVisited, strategyManager.ChangeBuffer);
                }

                strategyManager.ChangeBuffer.Push(this,
                    new LineHiddenPossibilitiesReportBuilder(newVisited, newMashed, col, Unit.Column));
            }
            else if (newVisited.Count < _type) RecursiveColumnMashing(strategyManager, i + 1, newMashed, newVisited,col);
        }
    }
    
    private void RecursiveMiniGridMashing(IStrategyManager strategyManager, int start, MiniGridPositions mashed,
        IPossibilities visited, int miniRow, int miniCol)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = strategyManager.MiniGridPositions(miniRow, miniCol, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Mash(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var position in newMashed)
                {
                    RemoveAllPossibilitiesExcept(position.Row, position.Col, newVisited, strategyManager.ChangeBuffer);
                }

                strategyManager.ChangeBuffer.Push(this,
                    new MiniGridHiddenPossibilitiesReportBuilder(newVisited, newMashed));
            }
            else if (newVisited.Count < _type) RecursiveMiniGridMashing(strategyManager, i + 1, newMashed,
                newVisited, miniRow, miniCol);
        }
    }

    private void RemoveAllPossibilitiesExcept(int row, int col, IPossibilities except,
        ChangeBuffer changeBuffer)
    {
        for (int number = 1; number <= 9; number++)
        {
            if (!except.Peek(number))
            {
                changeBuffer.AddPossibilityToRemove(number, row, col);
            }
        }
    }
}

public class LineHiddenPossibilitiesReportBuilder : IChangeReportBuilder
{
    private readonly IPossibilities _possibilities;
    private readonly LinePositions _linePos;
    private readonly int _unitNumber;
    private readonly Unit _unit;


    public LineHiddenPossibilitiesReportBuilder(IPossibilities possibilities, LinePositions linePos, int unitNumber, Unit unit)
    {
        _possibilities = possibilities;
        _linePos = linePos;
        _unitNumber = unitNumber;
        _unit = unit;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        var coords = new List<PossibilityCoordinate>();
        foreach (var possibility in _possibilities)
        {
            switch (_unit)
            {
                case Unit.Row :
                    foreach (var col in _linePos)
                    {
                        if(manager.Possibilities[_unitNumber, col].Peek(possibility))
                            coords.Add(new PossibilityCoordinate(_unitNumber, col, possibility));
                    }
                    break;
                case Unit.Column :
                    foreach (var row in _linePos)
                    {
                        if(manager.Possibilities[row, _unitNumber].Peek(possibility))
                            coords.Add(new PossibilityCoordinate(row, _unitNumber, possibility));
                    }
                    break;
            }  
        }
        
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(coord.Possibility, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class MiniGridHiddenPossibilitiesReportBuilder : IChangeReportBuilder
{
    private readonly IPossibilities _possibilities;
    private readonly MiniGridPositions _miniPos;

    public MiniGridHiddenPossibilitiesReportBuilder(IPossibilities possibilities, MiniGridPositions miniPos)
    {
        _possibilities = possibilities;
        _miniPos = miniPos;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        var coords = new List<PossibilityCoordinate>();
        foreach (var possibility in _possibilities)
        {
            foreach (var pos in _miniPos)
            {
                if(manager.Possibilities[pos.Row, pos.Col].Peek(possibility))
                    coords.Add(new PossibilityCoordinate(pos, possibility));
            }
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(coord.Possibility, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
                
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}