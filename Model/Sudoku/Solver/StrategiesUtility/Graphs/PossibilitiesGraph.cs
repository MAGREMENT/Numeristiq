using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Sudoku.Solver.Possibility;

namespace Model.Sudoku.Solver.StrategiesUtility.Graphs;

public class PossibilitiesGraph<T> : IEnumerable<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<PossibilitiesLink<T>>> _links = new();

    public void Add(T from, T to, Possibilities poss, LinkType type = LinkType.BiDirectional)
    {
        if (!_links.TryGetValue(from, out var hs))
        {
            hs = new HashSet<PossibilitiesLink<T>>();
            _links[from] = hs;
        }
        hs.Add(new PossibilitiesLink<T>(to, poss));
        
        if (type != LinkType.BiDirectional) return;
        
        if (!_links.TryGetValue(to, out hs))
        {
            hs = new HashSet<PossibilitiesLink<T>>();
            _links[to] = hs;
        }
        hs.Add(new PossibilitiesLink<T>(from, poss));
    }

    public IEnumerable<PossibilitiesLink<T>> GetLinks(T from)
    {
        return _links.TryGetValue(from, out var hs) ? hs : Enumerable.Empty<PossibilitiesLink<T>>();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _links.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public record PossibilitiesLink<T>(T To, IReadOnlyPossibilities Possibilities);