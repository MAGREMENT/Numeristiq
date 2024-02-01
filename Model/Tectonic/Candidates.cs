using System.Collections;
using System.Collections.Generic;

namespace Model.Tectonic;

public class Candidates : IEnumerable<int>
{
    private int _bits;
    private readonly int _max;
    
    public int Count { get; private set; }

    public Candidates(int max)
    {
        _max = max;
        Count = max;
        _bits = ((1 << _max) - 1) << 1;
    }

    public bool Remove(int n)
    {
        bool old = Peek(n);
        _bits &= ~(1 << n);
        if (old) Count--;
        return old;
    }
    
    public bool Peek(int number)
    {
        return ((_bits >> number) & 1) > 0;
    }

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 1; i <= _max; i++)
        {
            if (Peek(i)) yield return i;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}