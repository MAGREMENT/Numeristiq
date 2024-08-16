using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV3 : IOddagonSearchAlgorithm
{
    public int MaxLength { get; set; }
    public int MaxGuardians { get; set; }
    
    public List<AlmostOddagon> Search(ISudokuSolverData solverData, IGraph<CellPossibility, LinkStrength> graph)
    {
        return CycleBasis.Find(graph, (a, b, c, _, _) 
            => ConstructLoop(a, b, c, solverData));
    }

    private AlmostOddagon? ConstructLoop(Chain<CellPossibility, LinkStrength> fullPath, Chain<CellPossibility, LinkStrength> nonFullPath,
        int index, ISudokuSolvingState state)
    {
        var count = fullPath.Count + index + 1; 
        if (count > MaxLength || count % 2 != 1) return null;
        
        List<CellPossibility> elements = new();
        List<LinkStrength> links = new();
        HashSet<CellPossibility> guardians = new();

        for (int i = 0; i < fullPath.Count; i++)
        {
            var e = fullPath.Elements[i];
            if (guardians.Contains(e)) return null;

            elements.Add(e);
            if (i != fullPath.Count - 1 && !ProcessLink(state, e, fullPath.Elements[i + 1], guardians,
                    elements, links)) return null;
        }

        if (!ProcessLink(state, fullPath.Elements[^1], nonFullPath.Elements[index], guardians, elements, links)) return null;

        for (int i = index; i >= 0; i--)
        {
            var e = nonFullPath.Elements[i];
            if (guardians.Contains(e)) return null;

            elements.Add(e);
            if (i != 0 && !ProcessLink(state, e, nonFullPath.Elements[i - 1], guardians, elements, links)) return null;
        }

        return ProcessLink(state, nonFullPath.Elements[0], fullPath.Elements[0], guardians, elements, links)
            ? new AlmostOddagon(new Loop<CellPossibility, LinkStrength>(elements.ToArray(), links.ToArray()),
                guardians.ToArray())
            : null;
    }

    private bool ProcessLink(ISudokuSolvingState state, CellPossibility e1, CellPossibility e2,
        HashSet<CellPossibility> guardians, List<CellPossibility> elements, List<LinkStrength> links)
    {
        var buffer = OddagonSearcher.FindGuardians(state, e1, e2);
        var yes = false;
        foreach (var g in buffer)
        {
            yes = true;
            if(guardians.Contains(g)) continue;

            if (elements.Contains(g)) return false;

            guardians.Add(g);
            if (guardians.Count > MaxGuardians) return false;
        }

        links.Add(yes ? LinkStrength.Weak : LinkStrength.Strong);
        return true;
    }
}