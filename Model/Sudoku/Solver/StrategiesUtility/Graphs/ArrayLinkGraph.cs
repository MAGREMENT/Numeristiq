using System.Collections;
using System.Collections.Generic;
using Model.Sudoku.Utility;

namespace Model.Sudoku.Solver.StrategiesUtility.Graphs;

public class ArrayLinkGraph<T> : ILinkGraph<T> where T : notnull
{
    private readonly UniqueList<T>?[,,] _cp = new UniqueList<T>[9, 9, 2];
    private readonly Dictionary<T, UniqueList<T>[]> _other = new();
    
    public void Add(T from, T to, LinkStrength strength, LinkType type = LinkType.BiDirectional)
    {
        AddMono(from, to, strength);
        if (type == LinkType.BiDirectional) AddMono(to, from, strength);
    }

    private void AddMono(T from, T to, LinkStrength strength)
    {
        if (from is CellPossibility cp)
        {
            _cp[cp.Row, cp.Column, (int)strength] ??= new UniqueList<T>();
            _cp[cp.Row, cp.Column, (int)strength].Add(to);
        }
        else
        {
            if (!_other.TryGetValue(from, out var resume))
            {
                resume = new[] { new UniqueList<T>(), new UniqueList<T>() };
                _other[from] = resume;
            }
            resume[(int)strength - 1].Add(to);
        }
    }

    public IEnumerable<T> Neighbors(T from, LinkStrength strength)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<T> Neighbors(T from)
    {
        throw new System.NotImplementedException();
    }

    public bool AreNeighbors(T from, T to, LinkStrength strength)
    {
        throw new System.NotImplementedException();
    }

    public bool AreNeighbors(T from, T to)
    {
        throw new System.NotImplementedException();
    }

    public void Clear()
    {
        throw new System.NotImplementedException();
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}