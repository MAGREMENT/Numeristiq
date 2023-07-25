﻿using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class XCyclesStrategy : IStrategy
{
    public string Name { get; } = "XCycles";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;
    public int Score { get; set; }

    public void ApplyOnce(ISolverView solverView)
    {
        for (int n = 1; n <= 9; n++)
        {
            Dictionary<Coordinate, Coordinate>[] strongLinks = { new(), new(), new() };

            //Rows
            for (int row = 0; row < 9; row++)
            {
                var pos = solverView.PossibilityPositionsInRow(row, n);
                if (pos.Count == 2)
                {
                    int[] array = pos.ToArray();
                    strongLinks[0].Add(new Coordinate(row, array[0]), new Coordinate(row, array[1]));
                    strongLinks[0].Add(new Coordinate(row, array[1]), new Coordinate(row, array[0]));
                }
            }
            
            //Cols
            for (int col = 0; col < 9; col++)
            {
                var pos = solverView.PossibilityPositionsInColumn(col, n);
                if (pos.Count == 2)
                {
                    int[] array = pos.ToArray();
                    strongLinks[1].Add(new Coordinate(array[0], col), new Coordinate(array[1], col));
                    strongLinks[1].Add(new Coordinate(array[1], col), new Coordinate(array[0], col));
                }
            }
            
            //MiniGrids
            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var pos = solverView.PossibilityPositionsInMiniGrid(miniRow, miniCol, n);
                    if (pos.Count == 2)
                    {
                        int[][] array = pos.ToArray();
                        strongLinks[2].Add(new Coordinate(array[0][0], array[0][1]),
                            new Coordinate(array[1][0], array[1][1]));
                        strongLinks[2].Add(new Coordinate(array[1][0], array[1][1]),
                            new Coordinate(array[0][0], array[0][1]));
                    }
                }
            }

            for (int type = 0; type < 3; type++)
            {
                foreach (var start in strongLinks[type].Keys)
                {
                    List<Coordinate> visited = new() { start };
                    var next = strongLinks[type][start];
                    visited.Add(next);
                    foreach (var coord in SearchForWeakLink(solverView, strongLinks, next, n))
                    {
                        if (!visited.Contains(coord.Coordinate))
                        {
                            Search(solverView, strongLinks, coord, new List<Coordinate>(visited), n);
                        }
                    }
                }
            }
        }
    }

    private void Search(ISolverView solverView, Dictionary<Coordinate, Coordinate>[] strongLinks, CoordinateAndType current,
        List<Coordinate> visited, int number)
    {
        visited.Add(current.Coordinate);
        
        var next = strongLinks[current.Type][current.Coordinate];
        if (visited.Contains(next))
        {
            if (visited.Count >= 4 && visited[0].Equals(next)) ProcessOddLoop(solverView, visited, number);
            return;
        }
        visited.Add(next);
        
        bool noMore = true;
        foreach (var coord in SearchForWeakLink(solverView, strongLinks, next, number))
        {
            if (!visited.Contains(coord.Coordinate))
            {
                Search(solverView, strongLinks, coord, new List<Coordinate>(visited), number);
                noMore = false;
            }
            else if (coord.Coordinate.Equals(visited[0]))
            {
                ProcessFullLoop(solverView, visited, number);
                noMore = false;
            }
        }
        if(noMore) ProcessUnCompleteLoop(solverView, visited, number);
    }

    private void ProcessFullLoop(ISolverView solverView, List<Coordinate> visited, int number)
    {
        for (int i = 1; i < visited.Count - 1; i += 2)
        {
            foreach (var coord in visited[i].SharedSeenCells(visited[i + 1]))
            {
                solverView.RemovePossibility(number, coord.Row, coord.Col, this);
            }
        }
        
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            solverView.RemovePossibility(number, coord.Row, coord.Col, this);
        }
    }

    private void ProcessUnCompleteLoop(ISolverView solverView, List<Coordinate> visited, int number)
    {
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            solverView.RemovePossibility(number, coord.Row, coord.Col, this);
        }
    }

    private void ProcessOddLoop(ISolverView solverView, List<Coordinate> visited, int number)
    {
        solverView.AddDefinitiveNumber(number, visited[0].Row, visited[0].Col, this);
    }

    private IEnumerable<CoordinateAndType> SearchForWeakLink(ISolverView solverView, Dictionary<Coordinate, Coordinate>[] strongLinks,
        Coordinate current, int number)
    {
        HashSet<CoordinateAndType> result = new();

        for (int i = 0; i < 3; i++)
        {
            switch (i)
            {
                case 0 :
                    var posRow = solverView.PossibilityPositionsInRow(current.Row, number);
                    if (posRow.Count >= 2)
                    {
                        foreach (var col in posRow)
                        {
                            Coordinate coord = new Coordinate(current.Row, col);
                            if (strongLinks[0].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 0));
                            if (strongLinks[1].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 1));
                            if (strongLinks[2].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 2));
                        } 
                    }

                    break;
                case 1 :
                    var posCol = solverView.PossibilityPositionsInColumn(current.Col, number);
                    if (posCol.Count >= 2)
                    {
                        foreach (var row in posCol)
                        {
                            Coordinate coord = new Coordinate(row, current.Col);
                            if (strongLinks[0].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 0));
                            if (strongLinks[1].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 1));
                            if (strongLinks[2].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 2));
                        } 
                    }

                    break;
                case 2 :
                    var posMini = solverView.PossibilityPositionsInMiniGrid(current.Row / 3,
                        current.Col / 3, number);
                    if (posMini.Count >= 2)
                    {
                        foreach (var pos in posMini)
                        {
                            Coordinate coord = new Coordinate(pos[0], pos[1]);
                            if (strongLinks[0].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 0));
                            if (strongLinks[1].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 1));
                            if (strongLinks[2].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 2));
                        } 
                    }

                    break;
            }
        }

        foreach (var coord in result)
        {
            if (!(coord.Coordinate.Row == current.Row && coord.Coordinate.Col == current.Col)) yield return coord;
        }
    }
}

public class CoordinateAndType
{
    public CoordinateAndType(Coordinate coordinate, int type)
    {
        Coordinate = coordinate;
        Type = type;
    }

    public Coordinate Coordinate { get; }
    public int Type { get; }
}