using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV2 : IOddagonSearchAlgorithm
{
    private readonly HashSet<CellPossibility> _cpBuffer = new();
    private readonly HashSet<CellPossibility> _guardianBuffer = new();
    
    public int MaxLength { get; set; }
    public int MaxGuardians { get; set; }
    public List<AlmostOddagon> Search(ISudokuSolverData solverData, ILinkGraph<CellPossibility> graph)
    {
        var result = new List<AlmostOddagon>();
        foreach (var start in graph)
        {
            Search(solverData, graph, result, start);
        }

        return result;
    }

    private void Search(ISudokuSolverData data, ILinkGraph<CellPossibility> graph, List<AlmostOddagon> result,
        CellPossibility start)
    {
        Dictionary<CellPossibility, List<OddagonPath>> dic = new();
        Queue<OddagonPath> queue = new();

        queue.Enqueue(new OddagonPath(start));
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var last = current.Last();

            foreach (var friend in graph.Neighbors(last))
            {
                if(current.ContainsGuardian(friend)) continue;
                var (contains, isFirst) = current.ContainsExceptFirst(friend);
                if(contains) continue;

                if (isFirst)
                {
                    var odd = current.ToOddagon(data, MaxGuardians);
                    if (odd is not null) result.Add(odd);
                    continue;
                }

                if (dic.TryGetValue(friend, out var list))
                {
                    var wasRefusedOnce = false;
                    foreach (var otherPath in list)
                    {
                        var odd = TryMakeOddagon(data, current, otherPath, out var refused);
                        if (odd is not null) result.Add(odd);
                        if (refused) wasRefusedOnce = true;
                    }
                    
                    if(wasRefusedOnce || current.Length >= MaxLength) continue;
                    
                    var copy = current.Copy();
                    copy.Add(data, friend);
                    if(copy.GuardianCount > MaxGuardians) continue;

                    list.Add(copy);
                }
                else if(current.Length < MaxLength)
                {
                    var copy = current.Copy();
                    copy.Add(data, friend);
                    if(copy.GuardianCount > MaxGuardians) continue;
                    
                    list = new List<OddagonPath> { copy };
                    dic[friend] = list;
                    queue.Enqueue(copy);
                }
            }
        }
    }

    private AlmostOddagon? TryMakeOddagon(ISudokuSolvingState state, OddagonPath p1, OddagonPath p2, out bool refused)
    {
        _cpBuffer.Clear();
        _guardianBuffer.Clear();
        
        _cpBuffer.UnionWith(p1.EnumerateCells());
        _cpBuffer.UnionWith(p2.EnumerateCells());

        if (_cpBuffer.Count != p1.Length + p2.Length - 1)
        {
            refused = true;
            return null;
        }

        refused = false;
        if (_cpBuffer.Count > MaxLength || _cpBuffer.Count % 2 != 1) return null;

        _guardianBuffer.UnionWith(p1.EnumerateGuardians());
        _guardianBuffer.UnionWith(p2.EnumerateGuardians());
        var g = OddagonSearcher.FindGuardians(state, p1[^1], p2[^1]);
        _guardianBuffer.UnionWith(g);

        if (_guardianBuffer.Count > MaxGuardians) return null;

        var elements = new List<CellPossibility>(_cpBuffer.Count);
        var links = new List<LinkStrength>(_cpBuffer.Count);
        for (int i = 0; i < p1.Length - 1; i++)
        {
            elements.Add(p1[i]);
            links.Add(p1.LinkAt(i));
        }
        elements.Add(p1[^1]);
        links.Add(g.Any() ? LinkStrength.Weak : LinkStrength.Strong);
        for (int i = p2.Length - 1; i > 0; i--)
        {
            elements.Add(p2[i]);
            links.Add(p2.LinkAt(i - 1));
        }

        return new AlmostOddagon(new LinkGraphLoop<CellPossibility>(elements.ToArray(), links.ToArray()),
            _guardianBuffer.ToArray());
    }
}

public class OddagonPath
{
    private readonly List<CellPossibility> _cps;
    private readonly List<LinkStrength> _links;
    private readonly HashSet<CellPossibility> _guardians;

    public int Length => _cps.Count;
    public int GuardianCount => _guardians.Count;

    public OddagonPath(CellPossibility start)
    {
        _cps = new List<CellPossibility>();
        _links = new List<LinkStrength>();
        _guardians = new HashSet<CellPossibility>();
        _cps.Add(start);
    }

    private OddagonPath(List<CellPossibility> cps, List<LinkStrength> links,
        HashSet<CellPossibility> guardians)
    {
        _cps = cps;
        _links = links;
        _guardians = guardians;
    }

    public CellPossibility Last() => _cps[^1];

    public (bool, bool) ContainsExceptFirst(CellPossibility cp)
    {
        if (_cps[0] == cp) return (false, true);
        for (int i = 1; i < _cps.Count; i++)
        {
            if (_cps[i] == cp) return (true, true);
        }

        return (false, false);
    }
    public bool ContainsGuardian(CellPossibility cp) => _guardians.Contains(cp);
    public void Add(ISudokuSolvingState state, CellPossibility cp)
    {
        _cps.Add(cp);
        var before = _guardians.Count;
        _guardians.UnionWith(OddagonSearcher.FindGuardians(state, cp, _cps[^1]));
        _links.Add(before == _guardians.Count ? LinkStrength.Strong : LinkStrength.Weak);
    }
    public OddagonPath Copy() => new(new List<CellPossibility>(_cps), new List<LinkStrength>(_links),
        new HashSet<CellPossibility>(_guardians));
    public IEnumerable<CellPossibility> EnumerateCells() => _cps;
    public IEnumerable<CellPossibility> EnumerateGuardians() => _guardians;
    public CellPossibility this[int index] => _cps[index];
    public LinkStrength LinkAt(int index) => _links[index];

    public AlmostOddagon? ToOddagon(ISudokuSolvingState state, int maxGuardians)
    {
        if (_cps.Count % 2 != 1 && _cps.Count > 1) return null;

        var gBuffer = new HashSet<CellPossibility>(_guardians);
        var g = OddagonSearcher.FindGuardians(state, _cps[0], _cps[1]);
        gBuffer.UnionWith(g);
        if (gBuffer.Count > maxGuardians) return null;
        
        var lBuffer = new List<LinkStrength> { g.Any() ? LinkStrength.Weak : LinkStrength.Strong };
        return new AlmostOddagon(new LinkGraphLoop<CellPossibility>(_cps.ToArray(),
            lBuffer.ToArray()), gBuffer.ToArray());
    }
}