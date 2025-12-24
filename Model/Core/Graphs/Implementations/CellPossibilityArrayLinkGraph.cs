using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Core.Graphs.Implementations;

public class CellPossibilityArrayLinkGraph<TCollection> : IGraph<CellPossibility, LinkStrength> 
    where TCollection : IContainingCollection<CellPossibility>, new()
{
    private TCollection?[,,,] _edges;

    public CellPossibilityArrayLinkGraph(int rowCount, int colCount, int posCount)
    {
        _edges = new TCollection[rowCount, colCount, posCount, 2];
    }
    
    public void Add(CellPossibility from, CellPossibility to, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        AddMono(from, to, strength);
        if (type == LinkType.BiDirectional) AddMono(to, from, strength);
    }

    public IEnumerable<CellPossibility> GetNeighbors(CellPossibility from, LinkStrength strength)
    {
        if (strength == LinkStrength.Any) return GetNeighbors(from);
        
        return _edges[from.Row, from.Column, from.Possibility - 1, (int)strength - 1] ?? Enumerable.Empty<CellPossibility>();
    }

    public IEnumerable<CellPossibility> GetNeighbors(CellPossibility from)
    {
        var e1 = _edges[from.Row, from.Column, from.Possibility - 1, 0];
        var e2 = _edges[from.Row, from.Column, from.Possibility - 1, 1];

        if (e1 is null) return e2 ?? Enumerable.Empty<CellPossibility>();
        return e2 is null ? e1 : e1.Concat(e2);
    }

    public IEnumerable<EdgeTo<LinkStrength, CellPossibility>> GetNeighborsWithEdges(CellPossibility from)
    {
        foreach (var n in _edges[from.Row, from.Column, from.Possibility - 1, 0] ?? Enumerable.Empty<CellPossibility>())
        {
            yield return new EdgeTo<LinkStrength, CellPossibility>(LinkStrength.Strong, n);
        }
            
        foreach (var n in _edges[from.Row, from.Column, from.Possibility - 1, 1] ?? Enumerable.Empty<CellPossibility>())
        {
            yield return new EdgeTo<LinkStrength, CellPossibility>(LinkStrength.Weak, n);
        }
    }

    public bool AreNeighbors(CellPossibility from, CellPossibility to, LinkStrength strength)
    {
        if (strength == LinkStrength.Any) return AreNeighbors(from, to);
        
        var l = _edges[from.Row, from.Column, from.Possibility - 1, (int)strength - 1];
        return l is not null && l.Contains(to);
    }

    public bool AreNeighbors(CellPossibility from, CellPossibility to)
    {
        var l = _edges[from.Row, from.Column, from.Possibility - 1, 0];
        if (l is not null && l.Contains(to)) return true;
        
        l = _edges[from.Row, from.Column, from.Possibility - 1, 1];
        return l is not null && l.Contains(to);
    }

    public LinkStrength GetLinkBetween(CellPossibility from, CellPossibility to)
    {
        var list = _edges[from.Row, from.Column, from.Possibility - 1, 0];
        if (list is not null && list.Contains(to)) return LinkStrength.Strong;

        list = _edges[from.Row, from.Column, from.Possibility - 1, 1];
        if (list is not null && list.Contains(to)) return LinkStrength.Weak;

        return LinkStrength.None;
    }

    public void Clear()
    {
        _edges = new TCollection[9, 9, 9, 2];
    }
    
    public IEnumerator<CellPossibility> GetEnumerator()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                for (int p = 0; p < 9; p++)
                {
                    var weak = _edges[r, c, p, 0];
                    var strong = _edges[r, c, p, 1];
                    if ((weak is not null && weak.Count > 0) || (strong is not null && strong.Count > 0))
                        yield return new CellPossibility(r, c, p + 1);
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    //Private-----------------------------------------------------------------------------------------------------------
    
    private void AddMono(CellPossibility from, CellPossibility to, LinkStrength strength)
    {
        _edges[from.Row, from.Column, from.Possibility - 1, (int)strength - 1] ??= new TCollection();
        _edges[from.Row, from.Column, from.Possibility - 1, (int)strength - 1]!.Add(to);
    }
}