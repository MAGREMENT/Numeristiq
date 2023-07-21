using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies.AIC;

public class AlternatingInferenceChainStrategyV1 : IAlternatingInferenceChainStrategy
{
    public string Name { get; } = "Alternating inference chain";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Extreme;
    public int Score { get; set; }

    public long SearchCount { get; private set; }

    private readonly int _maxSearchCount;

    public AlternatingInferenceChainStrategyV1(int maxSearchCount)
    {
        _maxSearchCount = maxSearchCount;
    }

    public void ApplyOnce(ISolverView solverView)
    {
        SearchCount = 0;
        Dictionary<PossibilityCoordinate, LinkResume> map = new();

        SearchStrongLinks(solverView, map);
        SearchWeakLinks(solverView, map);

        foreach (var start in map)
        {
            if (start.Value.StrongLinks.Count == 0) continue;
            List<PossibilityCoordinate> visited = new() { start.Key };
            Search(solverView, map, visited);
        }
    }

    private void Search(ISolverView solverView, Dictionary<PossibilityCoordinate, LinkResume> map,
        List<PossibilityCoordinate> visited)
    {
        //Always start by strong link, so if visited.Count % 2 == 1 => Strong ; == 0 => Weak, but Strong can be Weak
        var last = visited[^1];
        var resume = map[last];

        if (SearchCount++ >= _maxSearchCount) return;

        if (visited.Count % 2 == 1)
        {
            foreach (var friend in resume.StrongLinks)
            {
                if (visited.Count >= 4 && visited[0].Equals(friend)) ProcessOddLoop(solverView, visited);
                if (!visited.Contains(friend))
                {
                   Search(solverView, map, new List<PossibilityCoordinate>(visited) { friend });
                }
            }
        }
        else
        {
            if (visited.Count >= 4)
                ProcessUnCompleteLoop(solverView, visited);
            
            foreach (var friend in resume.WeakLinks)
            {
                if (visited.Count >= 4 && visited[0].Equals(friend)) ProcessFullLoop(solverView, visited);
                if (!visited.Contains(friend))
                {
                    Search(solverView, map, new List<PossibilityCoordinate>(visited) { friend });
                }
            }
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
        
        foreach (var coord in one.SharedSeenCells(two))
        {
            solverView.RemovePossibility(one.Possibility, coord.Row, coord.Col, this);
        }
    }

    private void ProcessUnCompleteLoop(ISolverView solverView, List<PossibilityCoordinate> visited)
    {
        PossibilityCoordinate first = visited[0];
        PossibilityCoordinate last = visited[^1];

        if (first.Possibility == last.Possibility)
        {
            foreach (var coord in first.SharedSeenCells(last))
            {
                if (!visited.Contains(new PossibilityCoordinate(coord.Row, coord.Col, first.Possibility)))
                    solverView.RemovePossibility(first.Possibility, coord.Row, coord.Col, this);
            }
        }
        else if (first.ShareAUnit(last))
        {
            if (!visited.Contains(new PossibilityCoordinate(last.Row, last.Col, first.Possibility)))
                solverView.RemovePossibility(first.Possibility,last.Row, last.Col, this);

            if (!visited.Contains(new PossibilityCoordinate(first.Row, first.Col, last.Possibility)))
                solverView.RemovePossibility(last.Possibility, first.Row, first.Col, this);
        }
    }

    private void ProcessOddLoop(ISolverView solverView, List<PossibilityCoordinate> visited)
    {
        if (visited.Count % 2 != 1) return;
        solverView.AddDefinitiveNumber(visited[0].Possibility, visited[0].Row, visited[0].Col, this);
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

    private void SearchStrongLinks(ISolverView solverView, Dictionary<PossibilityCoordinate, LinkResume> map)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverView.Possibilities[row, col])
                {
                    LinkResume resume = new();

                    //Row
                    var ppir = solverView.PossibilityPositionsInRow(row, possibility);
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
                    var ppic = solverView.PossibilityPositionsInColumn(col, possibility);
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
                    var ppimn = solverView.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
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

                    if (solverView.Possibilities[row, col].Count == 2)
                    {
                        foreach (var pos in solverView.Possibilities[row, col])
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

    private void SearchWeakLinks(ISolverView solverView, Dictionary<PossibilityCoordinate, LinkResume> map)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverView.Possibilities[row, col])
                {
                    bool alreadyThere = map.TryGetValue(new PossibilityCoordinate(row, col, possibility), out var resume);
                    if (!alreadyThere) resume = new LinkResume();

                    //Row
                    var ppir = solverView.PossibilityPositionsInRow(row, possibility);
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
                    var ppic = solverView.PossibilityPositionsInColumn(col, possibility);
                    
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
                    var ppimn = solverView.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    
                    foreach (var pos in ppimn)
                    {
                        if (!(pos[0] == row && pos[1] == col))
                        {
                            var coord = new PossibilityCoordinate(pos[0], pos[1], possibility);
                            if (!map.TryGetValue(coord, out var links)) continue;
                            if (links.StrongLinks.Count > 0) resume!.WeakLinks.Add(coord);
                        }
                    }
                    

                    
                    foreach (var pos in solverView.Possibilities[row, col])
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
}

public class LinkResume
{
    public HashSet<PossibilityCoordinate> StrongLinks { get; }= new();
    public HashSet<PossibilityCoordinate> WeakLinks { get; } = new();

    public int Count => StrongLinks.Count + WeakLinks.Count;
}