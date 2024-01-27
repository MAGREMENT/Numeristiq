using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Global;

namespace Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

public class PositionsGraph<T> : IEnumerable<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<PositionsLink<T>>> _links = new();

    public void Add(T from, T to, Cell[] cells, LinkType type = LinkType.BiDirectional)
    {
        if (!_links.TryGetValue(from, out var hs))
        {
            hs = new HashSet<PositionsLink<T>>();
            _links[from] = hs;
        }
        hs.Add(new PositionsLink<T>(to, cells));
        
        if (type != LinkType.BiDirectional) return;
        
        if (!_links.TryGetValue(to, out hs))
        {
            hs = new HashSet<PositionsLink<T>>();
            _links[to] = hs;
        }
        hs.Add(new PositionsLink<T>(from, cells));
    }

    public IEnumerable<PositionsLink<T>> GetLinks(T from)
    {
        return _links.TryGetValue(from, out var hs) ? hs : Enumerable.Empty<PositionsLink<T>>();
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

public record PositionsLink<T>(T To, Cell[] Cells);