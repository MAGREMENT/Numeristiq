using System.Collections.Generic;
using System.Linq;
using Model.Strategies.StrategiesUtil;

namespace Model.Strategies;

public class AlternatingInferenceChainStrategy : IStrategy
{
    public int ModificationCount { get; private set; }
    public long SearchCount { get; private set; }
    
    public void ApplyOnce(ISolver solver)
    {
        Dictionary<PossibilityCoordinate, LinkResume> map = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solver.Possibilities[row, col])
                {
                    LinkResume resume = new();
                    
                    //Row
                    var ppir = solver.PossibilityPositionsInRow(row, possibility);
                    foreach (var c in ppir)
                    {
                        if (c != col)
                        {
                            var coord = new PossibilityCoordinate(row, c, possibility);
                            resume.WeakLinks.Add(coord);
                            if (ppir.Count == 2) resume.StrongLinks.Add(coord);
                        }
                    }
                    
                    //Col
                    var ppic = solver.PossibilityPositionsInColumn(col, possibility);
                    foreach (var r in ppic)
                    {
                        if (r != row)
                        {
                            var coord = new PossibilityCoordinate(r, col, possibility);
                            resume.WeakLinks.Add(coord);
                            if (ppic.Count == 2) resume.StrongLinks.Add(coord);
                        }
                    }
                    
                    //MiniGrids
                    var ppimn = solver.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    foreach (var pos in ppimn)
                    {
                        if (!(pos[0] == row && pos[1] == col))
                        {
                            var coord = new PossibilityCoordinate(pos[0], pos[1], possibility);
                            resume.WeakLinks.Add(coord);
                            if (ppimn.Count == 2) resume.StrongLinks.Add(coord);
                        }
                    }
                    
                    foreach (var pos in solver.Possibilities[row, col])
                    {
                        if (pos != possibility)
                        {
                            var coord = new PossibilityCoordinate(row, col, pos);
                            resume.WeakLinks.Add(coord);
                            if (solver.Possibilities[row, col].Count == 2) resume.StrongLinks.Add(coord);
                        }
                    }

                    if(resume.StrongLinks.Count > 0) map.Add(new PossibilityCoordinate(row, col, possibility), resume);
                }
            }
        }
        
        foreach (var start in map.Keys)
        {
            List<PossibilityCoordinate> visited = new() { start };
            if (Search(solver, map, visited)) return;
        }
    }

    private bool Search(ISolver solver, Dictionary<PossibilityCoordinate, LinkResume> map,
        List<PossibilityCoordinate> visited)
    {
        //Always start by strong link, so if visited.Count % 2 == 1 => Strong ; == 0 => Weak, but Strong can be Weak
        var last = visited[^1];
        if (!map.TryGetValue(last, out var resume)) return false;
        
        SearchCount++;

        if (visited.Count % 2 == 1)
        {
            foreach (var friend in resume.StrongLinks)
            {
                if (!visited.Contains(friend))
                {
                    if (Search(solver, map, new List<PossibilityCoordinate>(visited) { friend })) return true;
                }
                else if (visited.Count >= 4 && visited[0].Equals(friend) && ProcessOddLoop(solver, visited)) return true;
                
                if (visited.Count >= 4 && resume.WeakLinks.Contains(visited[0]) &&
                    ProcessUnCompleteLoop(solver, last)) return true;
            }
        }
        else
        {
            foreach (var friend in resume.WeakLinks)
            {
                if (!visited.Contains(friend))
                {
                    if (Search(solver, map, new List<PossibilityCoordinate>(visited) { friend })) return true;
                }
                else if (visited.Count >= 4 && visited[0].Equals(friend) && ProcessFullLoop(solver, visited)) return true;
            }
        }

        return false;
    }

    private bool ProcessFullLoop(ISolver solver, List<PossibilityCoordinate> visited)
    {
        //Always start with a strong link
        var wasProgressMade = false;
        for (int i = 1; i < visited.Count - 1; i += 2)
        {
            if (ProcessWeakLinkOfFullLoop(solver, visited[i], visited[i + 1]))
                wasProgressMade = true;
        }

        if (ProcessWeakLinkOfFullLoop(solver, visited[0], visited[^1]))
            wasProgressMade = true;
        return wasProgressMade;
    }

    private bool ProcessWeakLinkOfFullLoop(ISolver solver, PossibilityCoordinate one, PossibilityCoordinate two)
    {
        if (one.Row == two.Row && one.Col == two.Col)
        {
            return RemoveAllExcept(solver, one.Row, one.Col, one.Possibility, two.Possibility);
        }

        var wasProgressMade = false;
        foreach (var coord in one.SharedSeenCells(two))
        {
            if (solver.RemovePossibility(one.Possibility, coord.Row, coord.Col,
                    new InferenceChainLog(one.Possibility, coord.Row, coord.Col, 1)))
            {
                ModificationCount++;
                wasProgressMade = true;
            }
        }

        return wasProgressMade;

    }
    
    private bool ProcessUnCompleteLoop(ISolver solver, PossibilityCoordinate toRemove)
    {
        if (solver.RemovePossibility(toRemove.Possibility, toRemove.Row, toRemove.Col,
                new InferenceChainLog(toRemove.Possibility, toRemove.Row, toRemove.Col, 3)))
        {
            ModificationCount++;
            return true;
        }

        return false;
    }
    
    private bool ProcessOddLoop(ISolver solver, List<PossibilityCoordinate> visited)
    {
        if (visited.Count % 2 != 1) return false;
        if (solver.AddDefinitiveNumber(visited[0].Possibility, visited[0].Row, visited[0].Col,
                new InferenceChainLog(visited[0].Possibility, visited[0].Row, visited[0].Col, 2)))
        {
            ModificationCount++;
            return true;
        }

        return false;
    }

    private bool RemoveAllExcept(ISolver solver, int row, int col, params int[] except)
    {
        var wasProgressMade = false;
        foreach (var possibility in solver.Possibilities[row, col])
        {
            if (!except.Contains(possibility))
            {
                if (solver.RemovePossibility(possibility, row, col, new InferenceChainLog(possibility, row, col, 1)))
                {
                    ModificationCount++;
                    wasProgressMade = true;
                }
            }
        }

        return wasProgressMade;
    }
}

public class LinkResume
{
    public HashSet<PossibilityCoordinate> StrongLinks { get; }= new();
    public HashSet<PossibilityCoordinate> WeakLinks { get; } = new();
}

public class InferenceChainLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Extreme;

    public InferenceChainLog(int number, int row, int col, int type)
    {
        AsString = type == 2 ? $"[{row + 1}, {col + 1}] {number} added as definitive because of an alternating inference chain type {type}"
         : $"[{row + 1}, {col + 1}] {number} removed from possibilities because of an alternating inference chain type {type}";
    }
}