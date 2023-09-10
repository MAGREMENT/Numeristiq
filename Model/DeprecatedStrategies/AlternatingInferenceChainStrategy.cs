using System.Collections.Generic;
using System.Linq;
using Model.Solver;
using Model.StrategiesUtil;

namespace Model.DeprecatedStrategies;

public class AlternatingInferenceChainStrategy : IStrategy
{
    public string Name { get; } = "Alternating inference chain";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Extreme;
    public StatisticsTracker Tracker { get; } = new();

    public long SearchCount { get; private set; }

    private readonly int _maxSearchCount;
    private readonly int _maxLoopSize;

    public AlternatingInferenceChainStrategy(int maxSearchCount, int maxLoopSize)
    {
        _maxSearchCount = maxSearchCount;
        _maxLoopSize = maxLoopSize;
    }

    public AlternatingInferenceChainStrategy()
    {
        _maxSearchCount = int.MaxValue;
        _maxLoopSize = int.MaxValue;
    }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        SearchCount = 0;
        Dictionary<CellPossibility, LinkResume> map = new();

        SearchStrongLinks(strategyManager, map);
        SearchWeakLinks(strategyManager, map);

        foreach (var start in map)
        {
            if (start.Value.StrongLinks.Count == 0) continue;
            List<CellPossibility> visited = new() { start.Key };
            Search(strategyManager, map, visited);
        }
    }

    private void Search(IStrategyManager strategyManager, Dictionary<CellPossibility, LinkResume> map,
        List<CellPossibility> visited)
    {
        if (visited.Count > _maxLoopSize) return;
        //Always start by strong link, so if visited.Count % 2 == 1 => Strong ; == 0 => Weak, but Strong can be Weak
        var last = visited[^1];
        var resume = map[last];

        if (SearchCount++ >= _maxSearchCount) return;

        if (visited.Count % 2 == 1)
        {
            foreach (var friend in resume.StrongLinks)
            {
                if (visited.Count >= 4 && visited[0].Equals(friend)) ProcessOddLoop(strategyManager, visited);
                if (!visited.Contains(friend))
                {
                   Search(strategyManager, map, new List<CellPossibility>(visited) { friend });
                }
            }
        }
        else
        {
            if (visited.Count >= 4)
                ProcessUnCompleteLoop(strategyManager, visited);
            
            foreach (var friend in resume.WeakLinks)
            {
                if (visited.Count >= 4 && visited[0].Equals(friend)) ProcessFullLoop(strategyManager, visited);
                if (!visited.Contains(friend))
                {
                    Search(strategyManager, map, new List<CellPossibility>(visited) { friend });
                }
            }
        }
    }

    private void ProcessFullLoop(IStrategyManager strategyManager, List<CellPossibility> visited)
    {
        //Always start with a strong link
        for (int i = 1; i < visited.Count - 1; i += 2)
        {
            ProcessWeakLinkOfFullLoop(strategyManager, visited[i], visited[i + 1]);
        }

        ProcessWeakLinkOfFullLoop(strategyManager, visited[0], visited[^1]);
    }

    private void ProcessWeakLinkOfFullLoop(IStrategyManager strategyManager, CellPossibility one, CellPossibility two)
    {
        if (one.Row == two.Row && one.Col == two.Col)
        {
            RemoveAllExcept(strategyManager, one.Row, one.Col, one.Possibility, two.Possibility);
        }
        else
        {
            foreach (var coord in one.SharedSeenCells(two))
            {
                strategyManager.RemovePossibility(one.Possibility, coord.Row, coord.Col, this);
            }   
        }
    }

    private void ProcessUnCompleteLoop(IStrategyManager strategyManager, List<CellPossibility> visited)
    {
        CellPossibility first = visited[0];
        CellPossibility last = visited[^1];

        if (first.Possibility == last.Possibility)
        {
            foreach (var coord in first.SharedSeenCells(last))
            {
                if (!visited.Contains(new CellPossibility(coord.Row, coord.Col, first.Possibility)))
                    strategyManager.RemovePossibility(first.Possibility, coord.Row, coord.Col, this);
            }
        }
        else if (first.ShareAUnit(last))
        {
            if (!visited.Contains(new CellPossibility(last.Row, last.Col, first.Possibility)))
                strategyManager.RemovePossibility(first.Possibility,last.Row, last.Col, this);

            if (!visited.Contains(new CellPossibility(first.Row, first.Col, last.Possibility)))
                strategyManager.RemovePossibility(last.Possibility, first.Row, first.Col, this);
        }
    }

    private void ProcessOddLoop(IStrategyManager strategyManager, List<CellPossibility> visited)
    {
        if (visited.Count % 2 != 1) return;
        strategyManager.AddDefinitiveNumber(visited[0].Possibility, visited[0].Row, visited[0].Col, this);
    }

    private void RemoveAllExcept(IStrategyManager strategyManager, int row, int col, params int[] except)
    {
        foreach (var possibility in strategyManager.Possibilities[row, col])
        {
            if (!except.Contains(possibility))
            {
                strategyManager.RemovePossibility(possibility, row, col, this);
            }
        }
    }

    private void SearchStrongLinks(IStrategyManager strategyManager, Dictionary<CellPossibility, LinkResume> map)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    LinkResume resume = new();

                    //Row
                    var ppir = strategyManager.RowPositions(row, possibility);
                    if (ppir.Count == 2)
                    {
                        foreach (var c in ppir)
                        {
                            if (c != col)
                            {
                                resume.StrongLinks.Add(new CellPossibility(row, c, possibility));
                                break;
                            }
                        }
                    }


                    //Col
                    var ppic = strategyManager.ColumnPositions(col, possibility);
                    if (ppic.Count == 2)
                    {
                        foreach (var r in ppic)
                        {
                            if (r != row)
                            {
                                resume.StrongLinks.Add(new CellPossibility(r, col, possibility));
                                break;
                            }
                        }
                    }


                    //MiniGrids
                    var ppimn = strategyManager.MiniGridPositions(row / 3, col / 3, possibility);
                    if (ppimn.Count == 2)
                    {
                        foreach (var pos in ppimn)
                        {
                            if (!(pos.Row == row && pos.Col == col))
                            {
                                resume.StrongLinks.Add(new CellPossibility(pos, possibility));
                                break;
                            }
                        }
                    }

                    if (strategyManager.Possibilities[row, col].Count == 2)
                    {
                        foreach (var pos in strategyManager.Possibilities[row, col])
                        {
                            if (pos != possibility)
                            {
                                resume.StrongLinks.Add(new CellPossibility(row, col, pos));
                                break;
                            }
                        }
                    }

                    map.Add(new CellPossibility(row, col, possibility), resume);
                }
            }
        }
    }

    private void SearchWeakLinks(IStrategyManager strategyManager, Dictionary<CellPossibility, LinkResume> map)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    bool alreadyThere = map.TryGetValue(new CellPossibility(row, col, possibility), out var resume);
                    if (!alreadyThere) resume = new LinkResume();

                    //Row
                    var ppir = strategyManager.RowPositions(row, possibility);
                    foreach (var c in ppir)
                    {
                        if (c != col)
                        {
                            var coord = new CellPossibility(row, c, possibility);
                            if (!map.TryGetValue(coord, out var links)) continue;
                            if (links.StrongLinks.Count > 0) resume!.WeakLinks.Add(coord);
                        }
                    }
                    


                    //Col
                    var ppic = strategyManager.ColumnPositions(col, possibility);
                    
                    foreach (var r in ppic)
                    {
                        if (r != row)
                        {
                            var coord = new CellPossibility(r, col, possibility);
                            if (!map.TryGetValue(coord, out var links)) continue;
                            if (links.StrongLinks.Count > 0) resume!.WeakLinks.Add(coord);
                        }
                    }
                    


                    //MiniGrids
                    var ppimn = strategyManager.MiniGridPositions(row / 3, col / 3, possibility);
                    
                    foreach (var pos in ppimn)
                    {
                        if (!(pos.Row == row && pos.Col == col))
                        {
                            var coord = new CellPossibility(pos, possibility);
                            if (!map.TryGetValue(coord, out var links)) continue;
                            if (links.StrongLinks.Count > 0) resume!.WeakLinks.Add(coord);
                        }
                    }
                    

                    
                    foreach (var pos in strategyManager.Possibilities[row, col])
                    {
                        if (pos != possibility)
                        {
                            var coord = new CellPossibility(row, col, pos);
                            if (!map.TryGetValue(coord, out var links)) continue;
                            if (links.StrongLinks.Count > 0) resume!.WeakLinks.Add(coord);
                        }
                    }
                    

                    map[new CellPossibility(row, col, possibility)] = resume!;
                }
            }
        }
    }
}

public class LinkResume
{
    public HashSet<CellPossibility> StrongLinks { get; }= new();
    public HashSet<CellPossibility> WeakLinks { get; } = new();

    public int Count => StrongLinks.Count + WeakLinks.Count;
}

public class Path<T>
{
    public T First { get; }

    public Path(T first)
    {
        First = first;
    }
}