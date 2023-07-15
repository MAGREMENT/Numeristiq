using System.Collections.Generic;

namespace Model.Strategies;

/// <summary>
/// This class generalize the XWing, Swordfish and JellyFish strategy
/// </summary>
public class GridFormationStrategy : IStrategy
{
    private readonly int _type;
    private bool _lookingAtRows;

    public GridFormationStrategy(int type)
    {
        _type = type;
    }

    public void ApplyOnce(ISolver solver)
    { 
        for (int number = 1; number <= 9; number++)
        {
            Queue<ValuePositions> toSearch = new();
            _lookingAtRows = true;
            //Rows
            for (int row = 0; row < 9; row++)
            {
                Positions p = solver.PossibilityPositionsInRow(row, number);
                if (p.Count > 1 && p.Count <= _type) toSearch.Enqueue(new ValuePositions(p, row));
            }
            Search(solver, toSearch, number, _type);
            

            //Columns
            _lookingAtRows = false;
            for (int col = 0; col < 9; col++)
            {
                Positions p = solver.PossibilityPositionsInColumn(col, number);
                if (p.Count > 1 && p.Count <= _type) toSearch.Enqueue(new ValuePositions(p, col));
            }
            Search(solver, toSearch, number, _type);
            
        }
    }

    private void Search(ISolver solver, Queue<ValuePositions> toSearch, int number, int count)
    {
        while (toSearch.Count > 0)
        {
            ValuePositions first = toSearch.Dequeue();
            Positions visited = new();
            visited.Add(first.Value);
            RecursiveSearch(solver, new Queue<ValuePositions>(toSearch), visited,
                first.Positions, number, count - 1);
        }
    }

    private void RecursiveSearch(ISolver solver, Queue<ValuePositions> toSearch, Positions visited, Positions current,
        int number, int count)
    {
        while (toSearch.Count > 0)
        {
            ValuePositions dequeue = toSearch.Dequeue();
            Positions newCurrent = current.Mash(dequeue.Positions);
            visited.Add(dequeue.Value);
            if (count - 1 == 0)
            {
                if (newCurrent.Count == _type) Process(solver, visited, newCurrent, number);
            }
            else
            {
                if(newCurrent.Count <= _type)
                    RecursiveSearch(solver, new Queue<ValuePositions>(toSearch), visited.Copy(),
                        newCurrent, number, count - 1);
            }
        }
    }

    private void Process(ISolver solver, Positions visited, Positions toRemove, int number)
    {
        foreach (var first in toRemove)
        {
            for (int other = 0; other < 9; other++)
            {
                if (visited.Peek(other)) continue;
                
                int[] pos = _lookingAtRows ? new[] { other, first } : new[] { first, other };
                if (solver.Possibilities[pos[0], pos[1]].Peek(number))
                    solver.RemovePossibility(number, pos[0], pos[1],
                        new GridFormationLog(number, pos[0], pos[1], _type));
            }
        }
    }
}

public class GridFormationLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public GridFormationLog(int number, int row, int col, int type)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of grid formation {type}";
    }
}