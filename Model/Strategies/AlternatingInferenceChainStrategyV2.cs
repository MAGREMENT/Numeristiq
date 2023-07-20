using System.Collections.Generic;
using System.Linq;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class AlternatingInferenceChainStrategyV2 : IStrategy
{
    public string Name { get; } = "Alternating inference chain";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Extreme;
    
    public int ModificationCount { get; private set; }
    public long SearchCount { get; private set; }

    public void ApplyOnce(ISolverView solverView)
    {
        Dictionary<PossibilityCoordinate, LinkResume> map = new();

        SearchLinks(solverView, map);
        HashSet<PossibilityCoordinate> explored = new();

        foreach (var start in map)
        {
            //TODO
        }
    }

    private bool Search(ISolverView solverView, Dictionary<PossibilityCoordinate, LinkResume> map,
        List<PossibilityCoordinate> visited, bool wasStrong)
    {
        //Always start by strong link, so if visited.Count % 2 == 1 => Strong ; == 0 => Weak, but Strong can be Weak
        var last = visited[^1];
        var resume = map[last];

        SearchCount++;

        foreach (var stronk in resume.StrongLinks)
        {
            int contains = Contains(visited, stronk);
            if (contains == -1)
            {
                if(Search(solverView, map, new List<PossibilityCoordinate>(visited) { stronk }, true))
                    return true;
            }
            else if(visited.Count - contains >= 4)
            {
                //TODO
            }
        }

        return false;
    }

    private int Contains(List<PossibilityCoordinate> list, PossibilityCoordinate coord)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Equals(coord)) return i;
        }

        return -1;
    }

    private bool ProcessFullLoop(ISolverView solverView, List<PossibilityCoordinate> visited)
    {
        //Always start with a strong link
        var wasProgressMade = false;
        for (int i = 1; i < visited.Count - 1; i += 2)
        {
            if (ProcessWeakLinkOfFullLoop(solverView, visited[i], visited[i + 1]))
                wasProgressMade = true;
        }

        if (ProcessWeakLinkOfFullLoop(solverView, visited[0], visited[^1]))
            wasProgressMade = true;
        return wasProgressMade;
    }

    private bool ProcessWeakLinkOfFullLoop(ISolverView solverView, PossibilityCoordinate one, PossibilityCoordinate two)
    {
        if (one.Row == two.Row && one.Col == two.Col)
        {
            return RemoveAllExcept(solverView, one.Row, one.Col, 1, one.Possibility, two.Possibility);
        }

        var wasProgressMade = false;
        foreach (var coord in one.SharedSeenCells(two))
        {
            if (solverView.RemovePossibility(one.Possibility, coord.Row, coord.Col, this))
            {
                ModificationCount++;
                wasProgressMade = true;
            }
        }

        return wasProgressMade;

    }

    private bool ProcessUnCompleteLoop(ISolverView solverView, List<PossibilityCoordinate> visited)
    {
        PossibilityCoordinate first = visited[0];
        PossibilityCoordinate last = visited[^1];

        if (first.Possibility == last.Possibility)
        {
            foreach (var coord in first.SharedSeenCells(last))
            {
                if (!visited.Contains(new PossibilityCoordinate(coord.Row, coord.Col, first.Possibility)) &&
                    solverView.RemovePossibility(first.Possibility, coord.Row, coord.Col, this))
                {
                    ModificationCount++;
                    return true;
                }
            }
        }
        else if (first.ShareAUnit(last))
        {
            if (!visited.Contains(new PossibilityCoordinate(last.Row, last.Col, first.Possibility))
                && solverView.RemovePossibility(first.Possibility,last.Row, last.Col, this))
            {
                ModificationCount++;
                return true;
            }
            
            if (!visited.Contains(new PossibilityCoordinate(first.Row, first.Col, last.Possibility))
                && solverView.RemovePossibility(last.Possibility, first.Row, first.Col, this))
            {
                ModificationCount++;
                return true;
            }
        }

        return false;
    }

    private bool ProcessOddLoop(ISolverView solverView, List<PossibilityCoordinate> visited)
    {
        if (visited.Count % 2 != 1) return false;
        if (solverView.AddDefinitiveNumber(visited[0].Possibility, visited[0].Row, visited[0].Col, this))
        {
            ModificationCount++;
            return true;
        }

        return false;
    }

    private bool RemoveAllExcept(ISolverView solverView, int row, int col, int type, params int[] except)
    {
        var wasProgressMade = false;
        foreach (var possibility in solverView.Possibilities[row, col])
        {
            if (!except.Contains(possibility))
            {
                if (solverView.RemovePossibility(possibility, row, col, this))
                {
                    ModificationCount++;
                    wasProgressMade = true;
                }
            }
        }

        return wasProgressMade;
    }

    private void SearchLinks(ISolverView solverView, Dictionary<PossibilityCoordinate, LinkResume> map)
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
                    foreach (var c in ppir)
                    {
                        if (c != col)
                        {
                            var coord = new PossibilityCoordinate(row, c, possibility);
                            if (ppir.Count == 2) resume.StrongLinks.Add(coord);
                            resume.WeakLinks.Add(coord);
                        }
                    }


                    //Col
                    var ppic = solverView.PossibilityPositionsInColumn(col, possibility);
                    foreach (var r in ppic)
                    {
                        if (r != row)
                        {
                            var coord = new PossibilityCoordinate(r, col, possibility);
                            if(ppic.Count == 2) resume.StrongLinks.Add(coord);
                            resume.WeakLinks.Add(coord);
                        }
                    }
                    


                    //MiniGrids
                    var ppimn = solverView.PossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    foreach (var pos in ppimn)
                    {
                        if (!(pos[0] == row && pos[1] == col))
                        {
                            var coord = new PossibilityCoordinate(pos[0], pos[1], possibility);
                            if (ppimn.Count == 2) resume.StrongLinks.Add(coord);
                            resume.WeakLinks.Add(coord);
                        }
                    }

                   
                    foreach (var pos in solverView.Possibilities[row, col])
                    {
                        if (pos != possibility)
                        {
                            var coord = new PossibilityCoordinate(row, col, pos);
                            if(solverView.Possibilities[row, col].Count == 2) resume.StrongLinks.Add(coord);
                            resume.WeakLinks.Add(coord);
                        }
                    }

                    map.Add(new PossibilityCoordinate(row, col, possibility), resume);
                }
            }
        }
    }

}