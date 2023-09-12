using System.Collections.Generic;
using System.Linq;
using Model.Solver;
using Model.StrategiesUtil;

namespace Model.DeprecatedStrategies;

public class XCyclesStrategy : IStrategy
{
    public string Name { get; } = "XCycles";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int n = 1; n <= 9; n++)
        {
            Dictionary<Cell, Cell>[] strongLinks = { new(), new(), new() };

            //Rows
            for (int row = 0; row < 9; row++)
            {
                var pos = strategyManager.RowPositionsAt(row, n);
                if (pos.Count == 2)
                {
                    int[] array = pos.ToArray();
                    strongLinks[0].Add(new Cell(row, array[0]), new Cell(row, array[1]));
                    strongLinks[0].Add(new Cell(row, array[1]), new Cell(row, array[0]));
                }
            }
            
            //Cols
            for (int col = 0; col < 9; col++)
            {
                var pos = strategyManager.ColumnPositionsAt(col, n);
                if (pos.Count == 2)
                {
                    int[] array = pos.ToArray();
                    strongLinks[1].Add(new Cell(array[0], col), new Cell(array[1], col));
                    strongLinks[1].Add(new Cell(array[1], col), new Cell(array[0], col));
                }
            }
            
            //MiniGrids
            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var pos = strategyManager.MiniGridPositionsAt(miniRow, miniCol, n);
                    if (pos.Count == 2)
                    {
                        Cell[] array = pos.ToArray();
                        strongLinks[2].Add(new Cell(array[0].Row, array[0].Col),
                            new Cell(array[1].Row, array[1].Col));
                        strongLinks[2].Add(new Cell(array[1].Row, array[1].Col),
                            new Cell(array[0].Row, array[0].Col));
                    }
                }
            }

            for (int type = 0; type < 3; type++)
            {
                foreach (var start in strongLinks[type].Keys)
                {
                    List<Cell> visited = new() { start };
                    var next = strongLinks[type][start];
                    visited.Add(next);
                    foreach (var coord in SearchForWeakLink(strategyManager, strongLinks, next, n))
                    {
                        if (!visited.Contains(coord.Cell))
                        {
                            Search(strategyManager, strongLinks, coord, new List<Cell>(visited), n);
                        }
                    }
                }
            }
        }
    }

    private void Search(IStrategyManager strategyManager, Dictionary<Cell, Cell>[] strongLinks, CoordinateAndType current,
        List<Cell> visited, int number)
    {
        visited.Add(current.Cell);
        
        var next = strongLinks[current.Type][current.Cell];
        if (visited.Contains(next))
        {
            if (visited.Count >= 4 && visited[0].Equals(next)) ProcessOddLoop(strategyManager, visited, number);
            return;
        }
        visited.Add(next);
        
        bool noMore = true;
        foreach (var coord in SearchForWeakLink(strategyManager, strongLinks, next, number))
        {
            if (!visited.Contains(coord.Cell))
            {
                Search(strategyManager, strongLinks, coord, new List<Cell>(visited), number);
                noMore = false;
            }
            else if (coord.Cell.Equals(visited[0]))
            {
                ProcessFullLoop(strategyManager, visited, number);
                noMore = false;
            }
        }
        if(noMore) ProcessUnCompleteLoop(strategyManager, visited, number);
    }

    private void ProcessFullLoop(IStrategyManager strategyManager, List<Cell> visited, int number)
    {
        for (int i = 1; i < visited.Count - 1; i += 2)
        {
            foreach (var coord in visited[i].SharedSeenCells(visited[i + 1]))
            {
                strategyManager.RemovePossibility(number, coord.Row, coord.Col, this);
            }
        }
        
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            strategyManager.RemovePossibility(number, coord.Row, coord.Col, this);
        }
    }

    private void ProcessUnCompleteLoop(IStrategyManager strategyManager, List<Cell> visited, int number)
    {
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            strategyManager.RemovePossibility(number, coord.Row, coord.Col, this);
        }
    }

    private void ProcessOddLoop(IStrategyManager strategyManager, List<Cell> visited, int number)
    {
        strategyManager.AddSolution(number, visited[0].Row, visited[0].Col, this);
    }

    private IEnumerable<CoordinateAndType> SearchForWeakLink(IStrategyManager strategyManager, Dictionary<Cell, Cell>[] strongLinks,
        Cell current, int number)
    {
        HashSet<CoordinateAndType> result = new();

        for (int i = 0; i < 3; i++)
        {
            switch (i)
            {
                case 0 :
                    var posRow = strategyManager.RowPositionsAt(current.Row, number);
                    if (posRow.Count >= 2)
                    {
                        foreach (var col in posRow)
                        {
                            Cell coord = new Cell(current.Row, col);
                            if (strongLinks[0].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 0));
                            if (strongLinks[1].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 1));
                            if (strongLinks[2].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 2));
                        } 
                    }

                    break;
                case 1 :
                    var posCol = strategyManager.ColumnPositionsAt(current.Col, number);
                    if (posCol.Count >= 2)
                    {
                        foreach (var row in posCol)
                        {
                            Cell coord = new Cell(row, current.Col);
                            if (strongLinks[0].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 0));
                            if (strongLinks[1].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 1));
                            if (strongLinks[2].ContainsKey(coord)) result.Add(new CoordinateAndType(coord, 2));
                        } 
                    }

                    break;
                case 2 :
                    var posMini = strategyManager.MiniGridPositionsAt(current.Row / 3,
                        current.Col / 3, number);
                    if (posMini.Count >= 2)
                    {
                        foreach (var pos in posMini)
                        {
                            if (strongLinks[0].ContainsKey(pos)) result.Add(new CoordinateAndType(pos, 0));
                            if (strongLinks[1].ContainsKey(pos)) result.Add(new CoordinateAndType(pos, 1));
                            if (strongLinks[2].ContainsKey(pos)) result.Add(new CoordinateAndType(pos, 2));
                        } 
                    }

                    break;
            }
        }

        foreach (var coord in result)
        {
            if (!(coord.Cell.Row == current.Row && coord.Cell.Col == current.Col)) yield return coord;
        }
    }
}

public class CoordinateAndType
{
    public CoordinateAndType(Cell cell, int type)
    {
        Cell = cell;
        Type = type;
    }

    public Cell Cell { get; }
    public int Type { get; }
}