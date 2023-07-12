using System.Collections.Generic;

namespace Model;

public class BitPossibilities : IPossibilities
{
    private byte _possibilities = 0xFF;
    public int Count { private set; get; } = 9;

    public bool Remove(int number)
    {
        int index = number - 1;
        bool old = ((_possibilities >> index) & 1) > 0;
        _possibilities |= (byte) (1 << index);
        if (old) Count--;
        return old;
    }

    public void RemoveAll()
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

    public IPossibilities Mash(IPossibilities possibilities)
    {
        throw new System.NotImplementedException();
    }

    public bool Peek(int number)
    {
        return ((_possibilities >> number - 1) & 1) > 0;
    }

    public List<int> GetPossibilities()
    {
        throw new System.NotImplementedException();
    }

    public int GetFirst()
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _possibilities;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BitPossibilities p) return false;
        return p._possibilities == _possibilities;
    }

    public override string ToString()
    {
        string result = "[";
        for (int i = 1; i <= 9; i++)
        {
            if (Peek(i)) result += i + ", ";
        }

        if (result.Length != 1) result = result[..^2];
        return result + "]";
    }
}