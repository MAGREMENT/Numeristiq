using System;
using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies.AIC;

public class AlternatingInferenceChainStrategy : IStrategy
{
    public string Name { get; } = "Alternating inference chain";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Extreme;
    public int Score { get; set; }

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
        Dictionary<PossibilityCoordinate, LinkResume> map = new();

        SearchStrongLinks(strategyManager, map);
        SearchWeakLinks(strategyManager, map);

        foreach (var start in map)
        {
            if (start.Value.StrongLinks.Count == 0) continue;
            List<PossibilityCoordinate> visited = new() { start.Key };
            Search(strategyManager, map, visited);
        }
    }

    private void Search(IStrategyManager strategyManager, Dictionary<PossibilityCoordinate, LinkResume> map,
        List<PossibilityCoordinate> visited)
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
                   Search(strategyManager, map, new List<PossibilityCoordinate>(visited) { friend });
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
                    Search(strategyManager, map, new List<PossibilityCoordinate>(visited) { friend });
                }
            }
        }
    }

    private void ProcessFullLoop(IStrategyManager strategyManager, List<PossibilityCoordinate> visited)
    {
        //Always start with a strong link
        for (int i = 1; i < visited.Count - 1; i += 2)
        {
            ProcessWeakLinkOfFullLoop(strategyManager, visited[i], visited[i + 1]);
        }

        ProcessWeakLinkOfFullLoop(strategyManager, visited[0], visited[^1]);
    }

    private void ProcessWeakLinkOfFullLoop(IStrategyManager strategyManager, PossibilityCoordinate one, PossibilityCoordinate two)
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

    private void ProcessUnCompleteLoop(IStrategyManager strategyManager, List<PossibilityCoordinate> visited)
    {
        PossibilityCoordinate first = visited[0];
        PossibilityCoordinate last = visited[^1];

        if (first.Possibility == last.Possibility)
        {
            foreach (var coord in first.SharedSeenCells(last))
            {
                if (!visited.Contains(new PossibilityCoordinate(coord.Row, coord.Col, first.Possibility)))
                    strategyManager.RemovePossibility(first.Possibility, coord.Row, coord.Col, this);
            }
        }
        else if (first.ShareAUnit(last))
        {
            if (!visited.Contains(new PossibilityCoordinate(last.Row, last.Col, first.Possibility)))
                strategyManager.RemovePossibility(first.Possibility,last.Row, last.Col, this);

            if (!visited.Contains(new PossibilityCoordinate(first.Row, first.Col, last.Possibility)))
                strategyManager.RemovePossibility(last.Possibility, first.Row, first.Col, this);
        }
    }

    private void ProcessOddLoop(IStrategyManager strategyManager, List<PossibilityCoordinate> visited)
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

    private void SearchStrongLinks(IStrategyManager strategyManager, Dictionary<PossibilityCoordinate, LinkResume> map)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    LinkResume resume = new();

                    //Row
                    var ppir = strategyManager.PossibilityPositionsInRow(row, possibility);
                    if (ppir.Count == 2)
                    {
                        foreach (var c in ppir)
                        {
                            if (c != col)
                            {
                                resume.StrongLinks.Add(new PossibilityCoordinate(row, c, possibility));
                                break;
                            }
                        }
                    }


                    //Col
                    var ppic = strategyManager.PossibilityPositionsInColumn(col, possibility);
                    if (ppic.Count == 2)
                    {
                        foreach (var r in ppic)
                        {
                            if (r != row)
                            {
                                resume.StrongLinks.Add(new PossibilityCoordinate(r, col, possibility));
                                break;
                            }
                        }
                    }


                    //MiniGrids
                    var ppimn = strategyManager.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    if (ppimn.Count == 2)
                    {
                        foreach (var pos in ppimn)
                        {
                            if (!(pos[0] == row && pos[1] == col))
                            {
                                resume.StrongLinks.Add(new PossibilityCoordinate(pos[0], pos[1], possibility));
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
                                resume.StrongLinks.Add(new PossibilityCoordinate(row, col, pos));
                                break;
                            }
                        }
                    }

                    map.Add(new PossibilityCoordinate(row, col, possibility), resume);
                }
            }
        }
    }

    private void SearchWeakLinks(IStrategyManager strategyManager, Dictionary<PossibilityCoordinate, LinkResume> map)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    bool alreadyThere = map.TryGetValue(new PossibilityCoordinate(row, col, possibility), out var resume);
                    if (!alreadyThere) resume = new LinkResume();

                    //Row
                    var ppir = strategyManager.PossibilityPositionsInRow(row, possibility);
                    foreach (var c in ppir)
                    {
                        if (c != col)
                        {
                            var coord = new PossibilityCoordinate(row, c, possibility);
                            if (!map.TryGetValue(coord, out var links)) continue;
                            if (links.StrongLinks.Count > 0) resume!.WeakLinks.Add(coord);
                        }
                    }
                    


                    //Col
                    var ppic = strategyManager.PossibilityPositionsInColumn(col, possibility);
                    
                    foreach (var r in ppic)
                    {
                        if (r != row)
                        {
                            var coord = new PossibilityCoordinate(r, col, possibility);
                            if (!map.TryGetValue(coord, out var links)) continue;
                            if (links.StrongLinks.Count > 0) resume!.WeakLinks.Add(coord);
                        }
                    }
                    


                    //MiniGrids
                    var ppimn = strategyManager.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    
                    foreach (var pos in ppimn)
                    {
                        if (!(pos[0] == row && pos[1] == col))
                        {
                            var coord = new PossibilityCoordinate(pos[0], pos[1], possibility);
                            if (!map.TryGetValue(coord, out var links)) continue;
                            if (links.StrongLinks.Count > 0) resume!.WeakLinks.Add(coord);
                        }
                    }
                    

                    
                    foreach (var pos in strategyManager.Possibilities[row, col])
                    {
                        if (pos != possibility)
                        {
                            var coord = new PossibilityCoordinate(row, col, pos);
                            if (!map.TryGetValue(coord, out var links)) continue;
                            if (links.StrongLinks.Count > 0) resume!.WeakLinks.Add(coord);
                        }
                    }
                    

                    map[new PossibilityCoordinate(row, col, possibility)] = resume!;
                }
            }
        }
    }

    private int LoopToHash(List<PossibilityCoordinate> visited)
    {
        int result = 0;
        foreach (var coord in visited)
        {
            result ^= coord.GetHashCode();
        }

        return result;
    }
}

public class LinkResume
{
    public HashSet<PossibilityCoordinate> StrongLinks { get; }= new();
    public HashSet<PossibilityCoordinate> WeakLinks { get; } = new();

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