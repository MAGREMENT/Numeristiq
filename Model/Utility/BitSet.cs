using System;

namespace Model.Utility;

public class BitSet
{
    private const int BitSize = 64;
    
    private ulong[] _bits = new ulong[1];

    public void Set(int i)
    {
        if (i < 0) return;

        GrowIfNecessary(i);
        CheckedSet(i);
    }

    public void Unset(int i)
    {
        if (i < 0 || i > _bits.Length * BitSize) return;

        CheckedUnset(i);
    }

    public bool IsSet(int i)
    {
        if (i < 0 || i > _bits.Length * BitSize) return false;

        return CheckedIsSet(i);
    }

    public void Clear()
    {
        _bits = new ulong[1];
    }

    public void Insert(int i) //TODO test this
    {
        if (i < 0 || i > _bits.Length * BitSize) return;

        var n = i / BitSize;
        var buffer = 0ul;
        for (int j = 0; j < _bits.Length; j++)
        {
            if (j < n) continue;
            
            var bufferBuffer = (_bits[i] >> (BitSize - 1)) & 1ul;
            if (j == n)
            {
                var x = _bits[i];
                var y = x;
                var l = i % BitSize;
                var mask = (ulong)Math.Pow(2, l);

                x &= mask;
                y = (y & ~mask) << 1;
                _bits[i] = x | y;
            }
            else
            {
                _bits[i] = (_bits[i] << 1) | buffer;
            }

            buffer = bufferBuffer;
        }
    }

    public void Delete(int n)
    {
        //TODO
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private void GrowIfNecessary(int i)
    {
        var n = i / BitSize + 1;
        if (n > _bits.Length)
        {
            var buffer = new ulong[n];
            Array.Copy(_bits, 0, buffer, 0, _bits.Length);
            _bits = buffer;
        }
    }

    private void CheckedSet(int i)
    {
        _bits[i / BitSize] |= 1ul << (i % BitSize);
    }

    private void CheckedUnset(int i)
    {
        _bits[i / BitSize] &= ~(1ul << (i % BitSize));
    }

    private bool CheckedIsSet(int i)
    {
        return ((_bits[i / BitSize] >> (i % BitSize)) & 1ul) > 0;
    }
}