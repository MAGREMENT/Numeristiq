using System;
using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

/// <summary>
/// Hidden possibilities happens when n candidates are limited to n cells in a unit. In that case, each cell must have
/// one of the n candidates as a solution. Therefor, any other possibilities in those cells can be removed. As a side note,
/// not every possibility has to be present in every cell.
/// </summary>
public class HiddenPossibilitiesStrategy : AbstractStrategy
{
    public const string OfficialNameForType2 = "Hidden Double";
    public const string OfficialNameForType3 = "Hidden Triple";
    public const string OfficialNameForType4 = "Hidden Quad";

    private readonly int _type;

    public HiddenPossibilitiesStrategy(int type) : base("", StrategyDifficulty.None)
    {
        _type = type;
        switch (type)
        {
            case 2 : Name = OfficialNameForType2;
                Difficulty = StrategyDifficulty.Easy;
                break;
            case 3 : Name = OfficialNameForType3;
                Difficulty = StrategyDifficulty.Easy;
                break;
            case 4 : Name = OfficialNameForType4;
                Difficulty = StrategyDifficulty.Easy;
                break;
            default : throw new ArgumentException("Type not valid");
        }
    }
    
    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            RecursiveRowMashing(strategyManager, 1, new LinePositions(), IPossibilities.NewEmpty(), row);
        }

        for (int col = 0; col < 9; col++)
        {
            RecursiveColumnMashing(strategyManager, 1, new LinePositions(), IPossibilities.NewEmpty(), col);
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                RecursiveMiniGridMashing(strategyManager, 1, new MiniGridPositions(miniRow, miniCol),
                    IPossibilities.NewEmpty(), miniRow, miniCol);
            }
        }
    }

    private void RecursiveRowMashing(IStrategyManager strategyManager, int start, LinePositions mashed,
        IPossibilities visited, int row)
    {
        for (int i = start; i <= 9; i++)
        {
            var pos = strategyManager.RowPositionsAt(row, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Or(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var col in newMashed)
                {
                    RemoveAllPossibilitiesExcept(row, col, newVisited, strategyManager);
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
            var pos = strategyManager.ColumnPositionsAt(col, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Or(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var row in newMashed)
                {
                    RemoveAllPossibilitiesExcept(row, col, newVisited, strategyManager);
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
            var pos = strategyManager.MiniGridPositionsAt(miniRow, miniCol, i);
            if (pos.Count > _type || pos.Count == 0) continue;

            var newMashed = mashed.Or(pos);
            if(newMashed.Count > _type) continue;

            var newVisited = visited.Copy();
            newVisited.Add(i);

            if (newVisited.Count == _type && newMashed.Count == _type)
            {
                foreach (var position in newMashed)
                {
                    RemoveAllPossibilitiesExcept(position.Row, position.Col, newVisited, strategyManager);
                }

                strategyManager.ChangeBuffer.Push(this,
                    new MiniGridHiddenPossibilitiesReportBuilder(newVisited, newMashed));
            }
            else if (newVisited.Count < _type) RecursiveMiniGridMashing(strategyManager, i + 1, newMashed,
                newVisited, miniRow, miniCol);
        }
    }

    private void RemoveAllPossibilitiesExcept(int row, int col, IPossibilities except,
        IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            if (!except.Peek(number))
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, col);
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
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var coords = new List<CellPossibility>();
        foreach (var possibility in _possibilities)
        {
            foreach (var other in _linePos)
            {
                switch (_unit)
                {
                    case Unit.Row :
                        if(snapshot.PossibilitiesAt(_unitNumber, other).Peek(possibility))
                                coords.Add(new CellPossibility(_unitNumber, other, possibility));
                        break;
                    case Unit.Column :
                        if(snapshot.PossibilitiesAt(other, _unitNumber).Peek(possibility))
                                coords.Add(new CellPossibility(other, _unitNumber, possibility));
                        break;
                }  
            }
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(coord.Possibility, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        return $"The possibilities {_possibilities} are limited to the cells {_linePos.ToString(_unit, _unitNumber)} in" +
               $" {_unit.ToString().ToLower()} {_unitNumber + 1}, so any other candidates in those cells can be removed";
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var coords = new List<CellPossibility>();
        foreach (var possibility in _possibilities)
        {
            foreach (var pos in _miniPos)
            {
                if(snapshot.PossibilitiesAt(pos.Row, pos.Col).Peek(possibility))
                    coords.Add(new CellPossibility(pos, possibility));
            }
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(coord.Possibility, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
                
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
    
    private string Explanation()
    {
        return $"The possibilities {_possibilities} are limited to the cells {_miniPos} in" +
               $" mini grid {_miniPos.MiniGridNumber() + 1}, so any other candidates in those cells can be removed";
    }
}