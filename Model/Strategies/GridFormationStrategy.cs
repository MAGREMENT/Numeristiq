using System.Collections.Generic;
using Model.DeprecatedStrategies;
using Model.Positions;

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
    private bool _lookingAtRows;

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
            Queue<ValuePositions> toSearch = new();
            _lookingAtRows = true;
            //Rows
            for (int row = 0; row < 9; row++)
            {
                LinePositions p = strategyManager.PossibilityPositionsInRow(row, number);
                if (p.Count > 1 && p.Count <= _type) toSearch.Enqueue(new ValuePositions(p, row));
            }
            Search(strategyManager, toSearch, number, _type);
            

            //Columns
            _lookingAtRows = false;
            for (int col = 0; col < 9; col++)
            {
                LinePositions p = strategyManager.PossibilityPositionsInColumn(col, number);
                if (p.Count > 1 && p.Count <= _type) toSearch.Enqueue(new ValuePositions(p, col));
            }
            Search(strategyManager, toSearch, number, _type);
            
        }
    }

    public string GetExplanation(IChangeCauseFactory factory)
    {
        return "";
    }

    private void Search(IStrategyManager strategyManager, Queue<ValuePositions> toSearch, int number, int count)
    {
        while (toSearch.Count > 0)
        {
            ValuePositions first = toSearch.Dequeue();
            LinePositions visited = new();
            visited.Add(first.Value);
            RecursiveSearch(strategyManager, new Queue<ValuePositions>(toSearch), visited,
                first.Positions, number, count - 1);
        }
    }

    private void RecursiveSearch(IStrategyManager strategyManager, Queue<ValuePositions> toSearch, LinePositions visited, LinePositions current,
        int number, int count)
    {
        while (toSearch.Count > 0)
        {
            ValuePositions dequeue = toSearch.Dequeue();
            LinePositions newCurrent = current.Mash(dequeue.Positions);
            visited.Add(dequeue.Value);
            if (count - 1 == 0)
            {
                if (newCurrent.Count == _type) Process(strategyManager, visited, newCurrent, number);
            }
            else
            {
                if(newCurrent.Count <= _type)
                    RecursiveSearch(strategyManager, new Queue<ValuePositions>(toSearch), visited.Copy(),
                        newCurrent, number, count - 1);
            }
        }
    }

    private void Process(IStrategyManager strategyManager, LinePositions visited, LinePositions toRemove, int number)
    {
        foreach (var first in toRemove)
        {
            for (int other = 0; other < 9; other++)
            {
                if (visited.Peek(other)) continue;
                
                int[] pos = _lookingAtRows ? new[] { other, first } : new[] { first, other };
                if (strategyManager.Possibilities[pos[0], pos[1]].Peek(number))
                    strategyManager.RemovePossibility(number, pos[0], pos[1], this);
            }
        }
    }
}