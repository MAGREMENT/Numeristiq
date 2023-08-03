using System.Collections.Generic;
using System.Linq;
using Model.Positions;
using Model.Possibilities;

namespace Model.Strategies;

public class HiddenPossibilityStrategy : IStrategy
{
    public string Name { get; }

    public StrategyLevel Difficulty { get; }
    public int Score { get; set; }

    private readonly int _type;

    public HiddenPossibilityStrategy(int type)
    {
        _type = type;
        switch (type)
        {
            case 1 : Name = "Hidden single";
                Difficulty = StrategyLevel.Basic;
                break;
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
                    if(_type == 1)
                        strategyManager.AddDefinitiveNumber(entry.Value.First(), row, entry.Key.First(), this);
                    else
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
                    if(_type == 1)
                        strategyManager.AddDefinitiveNumber(entry.Value.First(), entry.Key.First(), col, this);
                    else
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
                        if (_type == 1)
                        {
                            var first = entry.Key.First();
                            strategyManager.AddDefinitiveNumber(entry.Value.First(), first[0], first[1], this);
                        }
                        else
                        {
                            var changeBuffer = strategyManager.CreateChangeBuffer(this, new MiniGridPositionsCauseFactory(
                                miniRow, miniCol, entry.Key, entry.Value));
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