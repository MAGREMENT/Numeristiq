using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies.AIC;

public class AlternatingInferenceChainStrategyV3 : IAlternatingInferenceChainStrategy
{
    public string Name => "Alternating inference chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }
    public long SearchCount { get; private set; }
    public void ApplyOnce(ISolverView solverView)
    {
        var map = new StrongLinkMap(solverView);

        foreach (var start in map)
        {
            Search(solverView, start, new List<PossibilityCoordinate>(), map);
        }
    }

    private void Search(ISolverView view, TypedCoordinate current, List<PossibilityCoordinate> visited, StrongLinkMap map)
    {
        SearchCount++;

        visited.Add(current.Coordinate);
        var friend = map.GetFriend(current);
        if(visited.Contains(friend))
        {
            if (visited.Count >= 4 && visited[0].Equals(friend))
                view.AddDefinitiveNumber(friend.Possibility, friend.Row, friend.Col, this);
            return;
        }

        visited.Add(friend);

        foreach (var next in map.FindWeakLinkedStrongLinks(view, friend))
        {
            if (visited.Contains(next.Coordinate))
            {
                if (visited.Count >= 4 && visited[0].Equals(next.Coordinate)) ProcessFullLoop(view, visited);
                continue;
            }
            Search(view, next, new List<PossibilityCoordinate>(visited), map);
        }

        if (visited.Count >= 4)
        {
            ProcessUnCompleteLoop(view, visited);
        }
    }

    private void ProcessFullLoop(ISolverView solverView, List<PossibilityCoordinate> visited)
    {
        //Always start with a strong link
        for (int i = 1; i < visited.Count - 1; i += 2)
        {
            ProcessWeakLinkOfFullLoop(solverView, visited[i], visited[i + 1]);
        }

        ProcessWeakLinkOfFullLoop(solverView, visited[0], visited[^1]);
    }

    private void ProcessWeakLinkOfFullLoop(ISolverView solverView, PossibilityCoordinate one, PossibilityCoordinate two)
    {
        if (one.Row == two.Row && one.Col == two.Col)
        {
            RemoveAllExcept(solverView, one.Row, one.Col, one.Possibility, two.Possibility);
        }
        else
        {
            foreach (var coord in one.SharedSeenCells(two))
            {
                solverView.RemovePossibility(one.Possibility, coord.Row, coord.Col, this);
            } 
        }
    }
    
    private void RemoveAllExcept(ISolverView solverView, int row, int col, params int[] except)
    {
        foreach (var possibility in solverView.Possibilities[row, col])
        {
            if (!except.Contains(possibility))
            {
                solverView.RemovePossibility(possibility, row, col, this);
            }
        }
    }

    private void ProcessUnCompleteLoop(ISolverView view, List<PossibilityCoordinate> visited)
    {
        var first = visited[0];
        var last = visited[^1];

        if (first.Possibility == last.Possibility)
        {
            foreach (var coord in first.SharedSeenCells(last))
            {
                if (!visited.Contains(new PossibilityCoordinate(coord.Row, coord.Col, first.Possibility)))
                    view.RemovePossibility(first.Possibility, coord.Row, coord.Col, this);
            }
        }
        else if (first.ShareAUnit(last))
        {
            if (!visited.Contains(new PossibilityCoordinate(last.Row, last.Col, first.Possibility)))
                view.RemovePossibility(first.Possibility,last.Row, last.Col, this);

            if (!visited.Contains(new PossibilityCoordinate(first.Row, first.Col, last.Possibility)))
                view.RemovePossibility(last.Possibility, first.Row, first.Col, this);
        }
    }
    
}

public class StrongLinkMap : IEnumerable<TypedCoordinate>
{
    private readonly Dictionary<PossibilityCoordinate, PossibilityCoordinate> _rows = new();
    private readonly Dictionary<PossibilityCoordinate, PossibilityCoordinate> _cols = new();
    private readonly Dictionary<PossibilityCoordinate, PossibilityCoordinate> _minis = new();
    private readonly Dictionary<PossibilityCoordinate, PossibilityCoordinate> _cells = new();

    public int Count => _rows.Count + _cols.Count + _minis.Count + _cells.Count;

    public StrongLinkMap(ISolverView solverView)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverView.Possibilities[row, col].Count == 2)
                {
                    var asArray = solverView.Possibilities[row, col].ToArray();
                    var one = new PossibilityCoordinate(row, col, asArray[0]);
                    var two = new PossibilityCoordinate(row, col, asArray[1]);
                    _cells.Add(one, two);
                    _cells.Add(two, one);
                }
            }
        }

        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var pos = solverView.PossibilityPositionsInRow(row, number);
                if (pos.Count == 2)
                {
                    var asArray = pos.ToArray();
                    var one = new PossibilityCoordinate(row, asArray[0], number);
                    var two = new PossibilityCoordinate(row, asArray[1], number);
                    _rows.Add(one, two);
                    _rows.Add(two, one);
                }
            }
            
            for (int col = 0; col < 9; col++)
            {
                var pos = solverView.PossibilityPositionsInColumn(col, number);
                if (pos.Count == 2)
                {
                    var asArray = pos.ToArray();
                    var one = new PossibilityCoordinate(asArray[0], col, number);
                    var two = new PossibilityCoordinate(asArray[1], col, number);
                    _cols.Add(one, two);
                    _cols.Add(two, one);
                }
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var pos = solverView.PossibilityPositionsInMiniGrid(miniRow, miniCol, number);
                    if (pos.Count == 2)
                    {
                        var asArray = pos.ToArray();
                        var one = new PossibilityCoordinate(asArray[0][0], asArray[0][1], number);
                        var two = new PossibilityCoordinate(asArray[1][0], asArray[1][1], number);
                        _minis.Add(one, two);
                        _minis.Add(two, one);
                    }
                }
            }
        }
    }

    public IEnumerable<TypedCoordinate> FindWeakLinkedStrongLinks(ISolverView view, PossibilityCoordinate coord)
    {
        if (view.Possibilities[coord.Row, coord.Col].Count >= 2)
        {
            foreach (var possibility in view.Possibilities[coord.Row, coord.Col])
            {
                if (possibility == coord.Possibility) continue;
                PossibilityCoordinate current = new PossibilityCoordinate(coord.Row, coord.Col, possibility);
                if (_rows.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Row);
                if (_cols.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Column);
                if (_minis.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.MiniGrid);
            }
        }

        var posR = view.PossibilityPositionsInRow(coord.Row, coord.Possibility);
        if (posR.Count >= 2)
        {
            foreach (var col in posR) 
            {
                if(col == coord.Col) continue;
                PossibilityCoordinate current = new PossibilityCoordinate(coord.Row, col, coord.Possibility);
                if (_rows.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Row);
                if (_cols.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Column);
                if (_minis.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.MiniGrid);
                if (_cells.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Cell);
            }
        }
        
        var posC = view.PossibilityPositionsInColumn(coord.Col, coord.Possibility);
        if (posC.Count >= 2)
        {
            foreach (var row in posC) 
            {
                if(row == coord.Row) continue;
                PossibilityCoordinate current = new PossibilityCoordinate(row, coord.Col, coord.Possibility);
                if (_rows.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Row);
                if (_cols.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Column);
                if (_minis.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.MiniGrid);
                if (_cells.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Cell);
            }
        }
        
        var posM = view.PossibilityPositionsInMiniGrid(coord.Row / 3, coord.Col / 3, coord.Possibility);
        if (posM.Count >= 2)
        {
            foreach (var pos in posM) 
            {
                if(coord.Row / 3 == pos[0] / 3 && coord.Col / 3 == pos[1] / 3) continue;
                PossibilityCoordinate current = new PossibilityCoordinate(pos[0], pos[1], coord.Possibility);
                if (_rows.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Row);
                if (_cols.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Column);
                if (_minis.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.MiniGrid);
                if (_cells.ContainsKey(current)) yield return new TypedCoordinate(current, StrongType.Cell);
            }
        }
    }

    public PossibilityCoordinate GetFriend(TypedCoordinate coord)
    {
        return coord.Type switch
        {
            StrongType.Row => _rows[coord.Coordinate],
            StrongType.Column => _cols[coord.Coordinate],
            StrongType.MiniGrid => _minis[coord.Coordinate],
            StrongType.Cell => _cells[coord.Coordinate],
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public IEnumerator<TypedCoordinate> GetEnumerator()
    {
        foreach (var pos in _rows.Keys)
        {
            yield return new TypedCoordinate(pos, StrongType.Row);
        }

        foreach (var pos in _cols.Keys)
        {
            yield return new TypedCoordinate(pos, StrongType.Column);
        }
        
        foreach (var pos in _minis.Keys)
        {
            yield return new TypedCoordinate(pos, StrongType.MiniGrid);
        }

        foreach (var pos in _cells.Keys)
        {
            yield return new TypedCoordinate(pos, StrongType.Cell);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class TypedCoordinate
{
    public TypedCoordinate(PossibilityCoordinate coordinate, StrongType type)
    {
        Coordinate = coordinate;
        Type = type;
    }

    public PossibilityCoordinate Coordinate { get; }
    
    public StrongType Type { get; }
}

public enum StrongType
{
    Row, Column, MiniGrid, Cell
}