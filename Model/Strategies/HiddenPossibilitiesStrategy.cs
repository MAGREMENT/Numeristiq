using System.Collections.Generic;
using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class HiddenPossibilitiesStrategy : IStrategy
{
    public string Name { get; }

    public StrategyLevel Difficulty { get; }
    public int Score { get; set; }

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
        //Rows
        for (int row = 0; row < 9; row++)
        {
            Dictionary<LinePositions, IPossibilities> possibilitiesToExamine = new();
            for (int number = 1; number <= 9; number++)
            {
                var positions = strategyManager.PossibilityPositionsInRow(row, number);
                if (positions.Count == _type)
                {
                    if(possibilitiesToExamine.TryGetValue(positions, out var possibilities)) possibilities.Add(number);
                    else
                    {
                        var buffer = IPossibilities.NewEmpty();
                        buffer.Add(number);
                        possibilitiesToExamine[positions] = buffer;
                    }
                }
            }

            foreach (var entry in possibilitiesToExamine)
            {
                if (entry.Value.Count == _type)
                {
                    var changeBuffer = strategyManager.CreateChangeBuffer(this,
                        new LineHiddenPossibilitiesReportWaiter(entry.Value, entry.Key, row, Unit.Row));
                    foreach (var col in entry.Key)
                    {
                        RemoveAllPossibilitiesExcept(row, col, entry.Value, changeBuffer);
                    }
                    changeBuffer.Push();
                    
                }
            }
        }
        
        //Columns
        for (int col = 0; col < 9; col++)
        {
            Dictionary<LinePositions, IPossibilities> possibilitiesToExamine = new();
            for (int number = 1; number <= 9; number++)
            {
                var positions = strategyManager.PossibilityPositionsInColumn(col, number);
                if (positions.Count == _type)
                {
                    if(possibilitiesToExamine.TryGetValue(positions, out var possibilities)) possibilities.Add(number);
                    else
                    {
                        var buffer = IPossibilities.NewEmpty();
                        buffer.Add(number);
                        possibilitiesToExamine[positions] = buffer;
                    }
                }
            }
            
            foreach (var entry in possibilitiesToExamine)
            {
                if (entry.Value.Count == _type)
                {
                    var changeBuffer = strategyManager.CreateChangeBuffer(this,
                        new LineHiddenPossibilitiesReportWaiter(entry.Value, entry.Key, col, Unit.Column));
                    foreach (var row in entry.Key)
                    {
                        RemoveAllPossibilitiesExcept(row, col, entry.Value, changeBuffer);
                    }
                    changeBuffer.Push();
                    
                }
            }
        }
        
        //MiniGrid
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                Dictionary<MiniGridPositions, IPossibilities> possibilitiesToExamine = new();
                for (int number = 1; number <= 9; number++)
                {
                    var positions = strategyManager.PossibilityPositionsInMiniGrid(miniRow, miniCol, number);
                    if (positions.Count == _type)
                    {
                        if(possibilitiesToExamine.TryGetValue(positions, out var possibilities)) possibilities.Add(number);
                        else
                        {
                            var buffer = IPossibilities.NewEmpty();
                            buffer.Add(number);
                            possibilitiesToExamine[positions] = buffer;
                        }
                    }
                }
                
                foreach (var entry in possibilitiesToExamine)
                {
                    if (entry.Value.Count == _type)
                    {
                        var changeBuffer = strategyManager.CreateChangeBuffer(this,
                            new MiniGridHiddenPossibilitiesReportWaiter(entry.Value, entry.Key));
                        foreach (var pos in entry.Key)
                        {
                            RemoveAllPossibilitiesExcept(pos[0], pos[1], entry.Value, changeBuffer);
                        }
                        changeBuffer.Push();
                    }
                }
            }
        }
    }

    /*public string GetExplanation(IChangeCauseFactory factory)
    {
        switch (factory)
        {
            case RowLinePositionsCauseFactory r:
                var cellsR = "";
                foreach (var col in r.Columns)
                {
                    cellsR += $"[{r.Row + 1}, {col + 1}] ";
                }
                return $"The numbers {r.Mashed} are only present on the cells {cellsR} in row {r.Row + 1}.\n" +
                       "Therefore any other possibility can be removed on those cells.";
            
            case ColumnLinePositionsCauseFactory c:
                var cellsC = "";
                foreach (var row in c.Rows)
                {
                    cellsC += $"[{row + 1}, {c.Col + 1}] ";
                }

                return $"The numbers {c.Mashed} are only present on the cells {cellsC} in column {c.Col + 1}.\n" +
                       "Therefore any other possibility can be removed on those cells.";
            
            case MiniGridPositionsCauseFactory m :
                var cellsM = "";
                foreach (var pos in m.GridPositions)
                {
                    cellsM += $"[{pos[0] + 1}, {pos[1] + 1}] ";
                }

                return $"The numbers {m.Mashed} are only present on the cells {cellsM} in mini grid {m.GridPositions.GetMiniGridNumber()}.\n" +
                       "Therefore any other possibility can be removed on those cells.";
            
            default: return "";
        }
    }*/

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

public class LineHiddenPossibilitiesReportWaiter : IChangeReportWaiter
{
    private readonly IPossibilities _possibilities;
    private readonly LinePositions _linePos;
    private readonly int _unitNumber;
    private readonly Unit _unit;


    public LineHiddenPossibilitiesReportWaiter(IPossibilities possibilities, LinePositions linePos, int unitNumber, Unit unit)
    {
        _possibilities = possibilities;
        _linePos = linePos;
        _unitNumber = unitNumber;
        _unit = unit;
    }
    
    public ChangeReport Process(List<SolverChange> changes, IChangeManager manager)
    {
        var coords = new List<Coordinate>();
        switch (_unit)
        {
            case Unit.Row :
                foreach (var col in _linePos)
                {
                    coords.Add(new Coordinate(_unitNumber, col));
                }
                break;
            case Unit.Column :
                foreach (var row in _linePos)
                {
                    coords.Add(new Coordinate(row, _unitNumber));
                }
                break;
        }
        
        return new ChangeReport(IChangeReportWaiter.ChangesToString(changes), lighter =>
        {
            foreach (var coord in coords)
            {
                foreach (var possibility in _possibilities)
                {
                    lighter.HighlightPossibility(possibility, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
                }
            }
            
            IChangeReportWaiter.HighlightChanges(lighter, changes);
        }, "");
    }
}

public class MiniGridHiddenPossibilitiesReportWaiter : IChangeReportWaiter
{
    private readonly IPossibilities _possibilities;
    private readonly MiniGridPositions _miniPos;

    public MiniGridHiddenPossibilitiesReportWaiter(IPossibilities possibilities, MiniGridPositions miniPos)
    {
        _possibilities = possibilities;
        _miniPos = miniPos;
    }

    public ChangeReport Process(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportWaiter.ChangesToString(changes), lighter =>
        {
            foreach (var pos in _miniPos)
            {
                foreach (var possibility in _possibilities)
                {
                    lighter.HighlightPossibility(possibility, pos[0], pos[1], ChangeColoration.CauseOffOne);
                }
            }
            
            IChangeReportWaiter.HighlightChanges(lighter, changes);
        }, "");
    }
}