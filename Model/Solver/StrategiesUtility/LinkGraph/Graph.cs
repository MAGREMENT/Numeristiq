using System.Collections.Generic;
using System.Linq;

namespace Model.Solver.StrategiesUtility.LinkGraph;

public class Graph<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<T>> _links = new();

    public void Add(T from, T to, LinkType type = LinkType.BiDirectional)
    {
        if (!_links.TryGetValue(from, out var hs))
        {
            hs = new HashSet<T>();
            _links[from] = hs;
        }
        hs.Add(to);
        
        if (type != LinkType.BiDirectional) return;
        
        if (!_links.TryGetValue(to, out hs))
        {
            hs = new HashSet<T>();
            _links[to] = hs;
        }
        hs.Add(from);
    }

    public IEnumerable<T> GetLinks(T from)
    {
        return _links.TryGetValue(from, out var hs) ? hs : Enumerable.Empty<T>();
    }
}