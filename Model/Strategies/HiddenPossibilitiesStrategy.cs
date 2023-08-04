using System.Collections.Generic;
using Model.Positions;
using Model.Possibilities;

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
                Difficulty = StrategyLevel.Medium;
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
                    var changeBuffer = strategyManager.CreateChangeBuffer(this, new RowLinePositionsCauseFactory(
                            row, entry.Key, entry.Value));
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
                    var changeBuffer = strategyManager.CreateChangeBuffer(this, new ColumnLinePositionsCauseFactory(
                            col, entry.Key, entry.Value));
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
                        var changeBuffer = strategyManager.CreateChangeBuffer(this, new MiniGridPositionsCauseFactory(
                                entry.Key, entry.Value));
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

    public string GetExplanation(IChangeCauseFactory factory)
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