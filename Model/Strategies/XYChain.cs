﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class XYChainStrategy : IStrategy
{
    public string Name { get; } = "XYChain";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;
    public int Score { get; set; }

    private readonly HashSet<PossibilityCoordinate> _used = new ();
    
    public void ApplyOnce(ISolverView solverView)
    
    {
        var map = new BiValueMap(solverView);
        _used.Clear();

        foreach (var start in map)
        {
            if (_used.Contains(start)) continue;
            Search(solverView, map, new HashSet<PossibilityCoordinate>(), start, start);
        }
    }

    private void Search(ISolverView solverView, BiValueMap map, HashSet<PossibilityCoordinate> visited,
        PossibilityCoordinate start, PossibilityCoordinate current)
    {
        PossibilityCoordinate friend = map.AssociatedCoordinate(current);
        if(friend.Possibility == start.Possibility) Process(solverView, start, friend);
        
        visited.Add(current);
        visited.Add(friend);
        
        foreach (var shared in map.AssociatedCoordinates(friend.Possibility))
        {
            if (!visited.Contains(shared) && shared.ShareAUnit(current))
            {
                Search(solverView, map, new HashSet<PossibilityCoordinate>(visited), start, shared);
            }
        }
    }

    private void Process(ISolverView solverView, PossibilityCoordinate start, PossibilityCoordinate end)
    {
        foreach (var coord in start.SharedSeenCells(end))
        {
            solverView.RemovePossibility(start.Possibility, coord.Row, coord.Col, this);
        }

        _used.Add(end);
    }
}

public class BiValueMap : IEnumerable<PossibilityCoordinate>
{
    private readonly Dictionary<PossibilityCoordinate, PossibilityCoordinate> _cells = new();
    private readonly Dictionary<int, HashSet<PossibilityCoordinate>> _map = new();

    public BiValueMap(ISolverView solverView)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverView.Possibilities[row, col].Count == 2)
                {
                    int[] possibilities = solverView.Possibilities[row, col].ToArray();
                    
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