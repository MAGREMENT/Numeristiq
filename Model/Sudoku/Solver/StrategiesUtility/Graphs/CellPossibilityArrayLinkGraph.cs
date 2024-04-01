using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Sudoku.Solver.StrategiesUtility.Graphs;

public class CellPossibilityArrayLinkGraph : ILinkGraph<CellPossibility>
{
    private UniqueList<CellPossibility>?[,,,] _cps = new UniqueList<CellPossibility>[9, 9, 9, 2];
    
    public void Add(CellPossibility from, CellPossibility to, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        AddMono(from, to, strength);
        if (type == LinkType.BiDirectional) AddMono(to, from, strength);
    }

    public IEnumerable<CellPossibility> Neighbors(CellPossibility from, LinkStrength strength)
    {
        if (strength == LinkStrength.Any) return Neighbors(from);
        
        return _cps[from.Row, from.Column, from.Possibility - 1, (int)strength - 1] ?? Enumerable.Empty<CellPossibility>();
    }

    public IEnumerable<CellPossibility> Neighbors(CellPossibility from)
    {
        foreach (var n in _cps[from.Row, from.Column, from.Possibility - 1, 0] ?? Enumerable.Empty<CellPossibility>())
        {
            yield return n;
        }
            
        foreach (var n in _cps[from.Row, from.Column, from.Possibility - 1, 1] ?? Enumerable.Empty<CellPossibility>())
        {
            yield return n;
        }
    }
 
    public bool AreNeighbors(CellPossibility from, CellPossibility to, LinkStrength strength)
    {
        if (strength == LinkStrength.Any) return AreNeighbors(from, to);
        
        var l = _cps[from.Row, from.Column, from.Possibility - 1, (int)strength - 1];
        return l is not null && l.Contains(to);
    }

    public bool AreNeighbors(CellPossibility from, CellPossibility to)
    {
        var l = _cps[from.Row, from.Column, from.Possibility - 1, 0];
        if (l is not null && l.Contains(to)) return true;
        
        l = _cps[from.Row, from.Column, from.Possibility - 1, 1];
        return l is not null && l.Contains(to);
    }

    public void Clear()
    {
        _cps = new UniqueList<CellPossibility>[9, 9, 9, 2];
    }
    
    public IEnumerator<CellPossibility> GetEnumerator()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                for (int p = 0; p < 9; p++)
                {
                    if (_cps[r, c, p, 0] is not null || _cps[r, c, p, 1] is not null)
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
        _cps[from.Row, from.Column, from.Possibility - 1, (int)strength - 1] ??= new UniqueList<CellPossibility>();
        _cps[from.Row, from.Column, from.Possibility - 1, (int)strength - 1]!.Add(to);
    }
}