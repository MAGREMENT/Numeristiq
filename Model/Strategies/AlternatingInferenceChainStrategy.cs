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
                    
                    //Row //TODO make less braindead
                    var ppir = solver.PossibilityPositionsInRow(row, possibility);
                    switch (ppir.Count)
                    {
                        case 2:
                        {
                            foreach (var c in ppir)
                            {
                                if (c != col)
                                {
                                    resume.StrongLinks.Add(new PossibilityCoordinate(row, c, possibility));
                                    break;
                                }
                            }

                            break;
                        }
                        case > 2:
                        {
                            foreach (var c in ppir)
                            {
                                if (c != col)
                                {
                                    resume.WeakLinks.Add(new PossibilityCoordinate(row, c, possibility));
                                }
                            }

                            break;
                        }
                    }
                    
                    //Col
                    var ppic = solver.PossibilityPositionsInColumn(col, possibility);
                    switch (ppic.Count)
                    {
                        case 2:
                        {
                            foreach (var r in ppic)
                            {
                                if (r != row)
                                {
                                    resume.StrongLinks.Add(new PossibilityCoordinate(r, col, possibility));
                                    break;
                                }
                            }

                            break;
                        }
                        case > 2:
                        {
                            foreach (var r in ppic)
                            {
                                if (r != row)
                                {
                                    resume.WeakLinks.Add(new PossibilityCoordinate(r, col, possibility));
                                }
                            }

                            break;
                        }
                    }
                    
                    //MiniGrids
                    var ppimn = solver.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    switch (ppimn.Count)
                    {
                        case 2:
                        {
                            foreach (var pos in ppimn)
                            {
                                if (!(pos[0] == row && pos[1] == col))
                                {
                                    resume.StrongLinks.Add(new PossibilityCoordinate(pos[0], pos[1], possibility));
                                    break;
                                }
                            }

                            break;
                        }
                        case > 2:
                        {
                            foreach (var pos in ppimn)
                            {
                                if (!(pos[0] == row && pos[1] == col))
                                {
                                    resume.WeakLinks.Add(new PossibilityCoordinate(pos[0], pos[1], possibility));
                                }
                            }

                            break;
                        }
                    }

                    switch (solver.Possibilities[row, col].Count)
                    {
                        //Cell
                        case 2:
                        {
                            foreach (var pos in solver.Possibilities[row, col])
                            {
                                if (pos != possibility)
                                {
                                    resume.StrongLinks.Add(new PossibilityCoordinate(row, col, pos));
                                    break;
                                }
                            }

                            break;
                        }
                        case > 2:
                        {
                            foreach (var pos in solver.Possibilities[row, col])
                            {
                                if (pos != possibility)
                                {
                                    resume.WeakLinks.Add(new PossibilityCoordinate(row, col, pos));
                                }
                            }

                            break;
                        }
                    }
                    
                    map.Add(new PossibilityCoordinate(row, col, possibility), resume);
                }
            }
        }
        
        foreach (var start in map.Keys)
        {
            List<PossibilityCoordinate> visited = new() { start };
            if (Search(solver, map, visited));
        }
    }

    private bool Search(ISolver solver, Dictionary<PossibilityCoordinate, LinkResume> map,
        List<PossibilityCoordinate> visited)
    {
        //Always start by strong link, so if visited.Count % 2 == 1 => Strong ; == 0 => Weak, but Strong can be Weak so
        //always look at strong link
        SearchCount++;
        bool stopped = true;
        foreach (var friend in map[visited[^1]].StrongLinks)
        {
            if (!visited.Contains(friend))
            {
                if (Search(solver, map, new List<PossibilityCoordinate>(visited) { friend })) return true;
                stopped = false;
            }
            else if (visited[0].Equals(friend))
            {
                if (ProcessOddLoop(solver, visited)) return true;
            }
        }
        if (visited.Count % 2 == 0)
        {
            foreach (var friend in map[visited[^1]].WeakLinks)
            {
                if (!visited.Contains(friend))
                {
                    if (Search(solver, map, new List<PossibilityCoordinate>(visited) { friend })) return true;
                    stopped = false;
                }
                else if (visited[0].Equals(friend))
                {
                    if (ProcessFullLoop(solver, visited)) return true;
                }
            }
        }

        if (stopped)
        {
            if (ProcessUnCompleteLoop(solver, visited)) return true;
        }

        return false;
    }

    private bool ProcessFullLoop(ISolver solver, List<PossibilityCoordinate> visited)
    {
        if (visited.Count < 4) return false;
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
    
    private bool ProcessUnCompleteLoop(ISolver solver, List<PossibilityCoordinate> visited)
    {
        if (visited.Count < 4 || visited.Count % 2 != 0 || visited[0].Possibility != visited[^1].Possibility) return false;
        var wasProgressMade = false;
        foreach (var coord in visited[0].SharedSeenCells(visited[^1]))
        {
            if (solver.RemovePossibility(visited[0].Possibility, coord.Row, coord.Col,
                    new InferenceChainLog(visited[0].Possibility, coord.Row, coord.Col, 3)))
            {
                ModificationCount++;
                wasProgressMade = true;
            }
        }

        return wasProgressMade;
    }
    
    private bool ProcessOddLoop(ISolver solver, List<PossibilityCoordinate> visited)
    {
        if (visited.Count < 4 || visited.Count % 2 != 1) return false;
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