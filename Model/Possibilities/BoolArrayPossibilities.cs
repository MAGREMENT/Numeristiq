using System;
using System.Collections;
using System.Collections.Generic;
using Model.Strategies;

namespace Model.Possibilities;

public class BoolArrayPossibilities : IPossibilities
{
    private bool[] _possibilities = { true, true, true, true, true, true, true, true, true};
    public int Count { private set; get; } = 9;
    
    public BoolArrayPossibilities(){}

    private BoolArrayPossibilities(BoolArrayPossibilities toCopy)
    {
        Array.Copy(toCopy._possibilities, _possibilities, _possibilities.Length);
        Count = toCopy.Count;
    }

    public bool Remove(int i)
    {
        var old = _possibilities[i - 1];
        _possibilities[i - 1] = false;
        if(old) Count--;
        return old;
    }

    public void RemoveAll()
    {
        _possibilities = new[] { false, false, false, false, false, false, false, false, false};

        Count = 0;
    }

    public void RemoveAll(int except)
    {
        throw new NotImplementedException();
    }

    public void RemoveAll(params int[] except)
    {
        RemoveAll();
        foreach (var i in except)
        {
            if (!_possibilities[i - 1]) Count++;
            _possibilities[i - 1] = true;
        }
    }

    public void RemoveAll(IEnumerable<int> except)
    {
        RemoveAll();
        foreach (var i in except)
        {
            if (!_possibilities[i - 1]) Count++;
            _possibilities[i - 1] = true;
        }
    }

    public IPossibilities Mash(IPossibilities possibilities)
    {
        return IPossibilities.DefaultMash(this, possibilities);
    }

    public bool Peek(int i)
    {
        return _possibilities[i - 1];
    }

    public bool PeekAll(IPossibilities poss)
    {
        return IPossibilities.DefaultPeekAll(this, poss);
    }

    public void Reset()
    {
        for (int i = 0; i < _possibilities.Length; i++)
        {
            _possibilities[i] = true;
        }

        Count = 9;
    }

    public void Add(int n)
    {
        throw new NotImplementedException();
    }

    public int GetFirst()
    {
        for (int i = 0; i < 9; i++)
        {
            if (_possibilities[i]) return i + 1;
        }

        return 0;
    }

    public IPossibilities Copy()
    {
        return new BoolArrayPossibilities(this);
    }

    public IEnumerable<BiValue> EachBiValue()
    {
        yield break; //TODO
    }

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < _possibilities.Length; i++)
        {
            if (_possibilities[i])
            {
                yield return i + 1;
            }
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BoolArrayPossibilities cp) return false;
        for (int i = 0; i < _possibilities.Length; i++)
        {
            if (_possibilities[i] != cp._possibilities[i]) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int result = 0;
        for (int i = 0; i < _possibilities.Length; i++)
        {
            if (_possibilities[i]) result |= 1 << i;
        }

        return result;
    }

    public override string ToString()
    {
        string result = "[";
        for (int i = 0; i < _possibilities.Length; i++)
        {
            if (_possibilities[i]) result += (i + 1) + ", ";
        }

        result = result.Length > 1 ? result[..^2] : result;
        return result + "]";
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}