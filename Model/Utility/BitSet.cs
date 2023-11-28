using System;
using System.Text;

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

    public void Insert(int i)
    {
        if (i < 0 || i > _bits.Length * BitSize) return;

        var n = i / BitSize;
        var buffer = 0ul;
        for (int j = 0; j < _bits.Length; j++)
        {
            if (j < n) continue;
            
            var bufferBuffer = (_bits[j] >> (BitSize - 1)) & 1ul;
            if (j == n)
            {
                var x = _bits[j];
                var y = x;
                var l = i % BitSize;
                var mask = GetMask(l);

                x &= mask;
                y = (y << 1) & ~mask;
                _bits[j] = x | y;
            }
            else
            {
                _bits[j] = (_bits[j] << 1) | buffer;
            }

            buffer = bufferBuffer;
        }
    }

    public void Delete(int i)
    {
        if (i < 0 || i > _bits.Length * BitSize) return;

        var n = i / BitSize;
        for (int j = 0; j < _bits.Length; j++)
        {
            if (j < n) continue;
            
            if (j == n)
            {
                var x = _bits[j];
                var y = x;
                var l = i % BitSize;
                var mask = GetMask(l);

                x &= mask;
                y = (y >> 1) & ~mask;
                _bits[j] = x | y;
            }
            else
            {
                var before = j + 1 < _bits.Length ? _bits[j + 1] & 1 : 0;
                _bits[j] = (_bits[j] >> 1) | (before << 63);
            }
            
        }
    }

    private ulong GetMask(int to)
    {
        return ~(ulong.MaxValue << to);
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var l in _bits)
        {
            for (int i = 0; i < 64; i++)
            {
                builder.Append(((l >> i) & 1) > 0 ? "1" : "0");
            }
        }
        
        return builder.ToString();
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