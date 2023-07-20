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

    public void ApplyOnce(ISolverView solverView)
    { 
        for (int number = 1; number <= 9; number++)
        {
            Queue<ValuePositions> toSearch = new();
            _lookingAtRows = true;
            //Rows
            for (int row = 0; row < 9; row++)
            {
                LinePositions p = solverView.PossibilityPositionsInRow(row, number);
                if (p.Count > 1 && p.Count <= _type) toSearch.Enqueue(new ValuePositions(p, row));
            }
            Search(solverView, toSearch, number, _type);
            

            //Columns
            _lookingAtRows = false;
            for (int col = 0; col < 9; col++)
            {
                LinePositions p = solverView.PossibilityPositionsInColumn(col, number);
                if (p.Count > 1 && p.Count <= _type) toSearch.Enqueue(new ValuePositions(p, col));
            }
            Search(solverView, toSearch, number, _type);
            
        }
    }

    private void Search(ISolverView solverView, Queue<ValuePositions> toSearch, int number, int count)
    {
        while (toSearch.Count > 0)
        {
            ValuePositions first = toSearch.Dequeue();
            LinePositions visited = new();
            visited.Add(first.Value);
            RecursiveSearch(solverView, new Queue<ValuePositions>(toSearch), visited,
                first.Positions, number, count - 1);
        }
    }

    private void RecursiveSearch(ISolverView solverView, Queue<ValuePositions> toSearch, LinePositions visited, LinePositions current,
        int number, int count)
    {
        while (toSearch.Count > 0)
        {
            ValuePositions dequeue = toSearch.Dequeue();
            LinePositions newCurrent = current.Mash(dequeue.Positions);
            visited.Add(dequeue.Value);
            if (count - 1 == 0)
            {
                if (newCurrent.Count == _type) Process(solverView, visited, newCurrent, number);
            }
            else
            {
                if(newCurrent.Count <= _type)
                    RecursiveSearch(solverView, new Queue<ValuePositions>(toSearch), visited.Copy(),
                        newCurrent, number, count - 1);
            }
        }
    }

    private void Process(ISolverView solverView, LinePositions visited, LinePositions toRemove, int number)
    {
        foreach (var first in toRemove)
        {
            for (int other = 0; other < 9; other++)
            {
                if (visited.Peek(other)) continue;
                
                int[] pos = _lookingAtRows ? new[] { other, first } : new[] { first, other };
                if (solverView.Possibilities[pos[0], pos[1]].Peek(number))
                    solverView.RemovePossibility(number, pos[0], pos[1], this);
            }
        }
    }
}