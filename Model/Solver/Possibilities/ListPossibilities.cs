using System.Collections;
using System.Collections.Generic;
using Model.Solver.Strategies;

namespace Model.Solver.Possibilities;

public class ListPossibilities : IPossibilities //TODO : do & test this
{
    private readonly int[] _possibilities = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    public int Count { get; private set; } = 9;
    
    
    public IEnumerator<int> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public int GetFirst()
    {
        throw new System.NotImplementedException();
    }

    public IPossibilities Or(IReadOnlyPossibilities possibilities)
    {
        throw new System.NotImplementedException();
    }

    public IPossibilities And(IReadOnlyPossibilities possibilities)
    {
        throw new System.NotImplementedException();
    }

    public bool Peek(int n)
    {
        throw new System.NotImplementedException();
    }

    public bool PeekAll(IReadOnlyPossibilities poss)
    {
        throw new System.NotImplementedException();
    }

    public bool PeekAny(IReadOnlyPossibilities poss)
    {
        throw new System.NotImplementedException();
    }

    public bool PeekOnlyOne(IReadOnlyPossibilities poss)
    {
        throw new System.NotImplementedException();
    }

    public IPossibilities Copy()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<BiValue> EachBiValue()
    {
        throw new System.NotImplementedException();
    }

    public bool Remove(int n)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAll()
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAll(int except)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAll(params int[] except)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAll(IEnumerable<int> except)
    {
        throw new System.NotImplementedException();
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }

    public void Add(int n)
    {
        throw new System.NotImplementedException();
    }

    public void Add(IReadOnlyPossibilities possibilities)
    {
        IPossibilities.DefaultAdd(this, possibilities);
    }
}