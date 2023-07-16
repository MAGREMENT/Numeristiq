using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Strategies.StrategiesUtil;

namespace Model.Strategies;

public class XYChainStrategy : IStrategy
{
    private readonly HashSet<PossibilityCoordinate> _used = new ();
    
    public void ApplyOnce(ISolver solver)
    
    {
        var map = new BiValueMap(solver);
        _used.Clear();

        foreach (var start in map)
        {
            if (_used.Contains(start)) continue;
            Search(solver, map, new HashSet<PossibilityCoordinate>(), start, start);
        }
    }

    private void Search(ISolver solver, BiValueMap map, HashSet<PossibilityCoordinate> visited,
        PossibilityCoordinate start, PossibilityCoordinate current)
    {
        PossibilityCoordinate friend = map.AssociatedCoordinate(current);
        if(friend.Possibility == start.Possibility) Process(solver, start, friend);
        
        visited.Add(current);
        visited.Add(friend);
        
        foreach (var shared in map.AssociatedCoordinates(friend.Possibility))
        {
            if (!visited.Contains(shared) && shared.ShareAUnit(current))
            {
                Search(solver, map, new HashSet<PossibilityCoordinate>(visited), start, shared);
            }
        }
    }

    private void Process(ISolver solver, PossibilityCoordinate start, PossibilityCoordinate end)
    {
        foreach (var coord in start.SharedSeenCells(end))
        {
            solver.RemovePossibility(start.Possibility, coord.Row, coord.Col,
                new XYChainLog(start.Possibility, coord.Row, coord.Col));
        }

        _used.Add(end);
    }
}

public class PossibilityCoordinate : Coordinate
{
    public int Possibility { get; }
    
    public PossibilityCoordinate(int row, int col, int possibility) : base(row, col)
    {
        Possibility = possibility;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, Row, Col);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PossibilityCoordinate pc) return false;
        return pc.Possibility == Possibility && pc.Row == Row && pc.Col == Col;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Col + 1} => {Possibility}]";
    }
}

public class BiValueMap : IEnumerable<PossibilityCoordinate>
{
    private readonly Dictionary<PossibilityCoordinate, PossibilityCoordinate> _cells = new();
    private readonly Dictionary<int, HashSet<PossibilityCoordinate>> _map = new();

    public BiValueMap(ISolver solver)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Possibilities[row, col].Count == 2)
                {
                    int[] possibilities = solver.Possibilities[row, col].ToArray();
                    
                    PossibilityCoordinate first = new PossibilityCoordinate(row, col, possibilities[0]);
                    PossibilityCoordinate second = new PossibilityCoordinate(row, col, possibilities[1]);

                    _cells.Add(first, second);
                    _cells.Add(second, first);
                    
                    if (!_map.TryAdd(possibilities[0], new HashSet<PossibilityCoordinate> { first }))
                        _map[possibilities[0]].Add(first);
                    if (!_map.TryAdd(possibilities[1], new HashSet<PossibilityCoordinate> { second }))
                        _map[possibilities[1]].Add(second);
                }
            }
        }
    }

    public IEnumerator<PossibilityCoordinate> GetEnumerator()
    {
        return _cells.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public HashSet<PossibilityCoordinate> AssociatedCoordinates(int possibility)
    {
        return _map.TryGetValue(possibility, out var result) ?
            result : new HashSet<PossibilityCoordinate>();
    }

    public PossibilityCoordinate AssociatedCoordinate(PossibilityCoordinate coord)
    {
        return _cells[coord];
    }
}

public class XYChainLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public XYChainLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of XYChain";
    }
}