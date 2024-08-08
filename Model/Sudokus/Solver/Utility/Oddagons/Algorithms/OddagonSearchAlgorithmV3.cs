using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV3 : IOddagonSearchAlgorithm
{
    public int MaxLength { get; set; }
    public int MaxGuardians { get; set; }
    
    public List<AlmostOddagon> Search(ISudokuSolverData solverData, ILinkGraph<CellPossibility> graph)
    {
        return CycleBasis.Find(graph, (a, b, c) 
            => ConstructLoop(a, b, c, solverData));
    }

    private AlmostOddagon? ConstructLoop(List<CellPossibility> fullPath, List<CellPossibility> nonFullPath,
        int index, ISudokuSolvingState state)
    {
        if (fullPath.Count + index + 1 > MaxLength) return null;
        
        List<CellPossibility> elements = new();
        List<LinkStrength> links = new();
        HashSet<CellPossibility> guardians = new();

        for (int i = 0; i < fullPath.Count; i++)
        {
            var e = fullPath[i];
            if (guardians.Contains(e)) return null;

            elements.Add(e);
            if (i != fullPath.Count - 1 && ProcessLink(state, e, fullPath[i + 1], guardians,
                    elements, links)) return null;
        }

        if (!ProcessLink(state, fullPath[^1], nonFullPath[index], guardians, elements, links)) return null;

        for (int i = index; i >= 0; i--)
        {
            var e = nonFullPath[i];
            if (guardians.Contains(e)) return null;

            elements.Add(e);
            if (i != 0 && ProcessLink(state, e, nonFullPath[i - 1], guardians, elements, links)) return null;
        }

        return ProcessLink(state, nonFullPath[0], fullPath[0], guardians, elements, links)
            ? new AlmostOddagon(new LinkGraphLoop<CellPossibility>(elements.ToArray(), links.ToArray()),
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