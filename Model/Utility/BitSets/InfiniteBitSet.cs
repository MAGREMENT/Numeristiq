using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Model.Utility.BitSets;

public class InfiniteBitSet : IEnumerable<int>
{
    private const int BitSize = 64;
    
    private ulong[] _bits = new ulong[1];
    
    public int Count { get; private set; }

    public void Set(int i)
    {
        if (i < 0) return;
        
        GrowIfNecessary(i / BitSize + 1);
        if (!IsSet(i)) Count++;
        _bits[i / BitSize] |= 1ul << (i % BitSize);
    }

    public void Unset(int i)
    {
        if (i < 0 || i > _bits.Length * BitSize) return;

        if (IsSet(i)) Count--;
        _bits[i / BitSize] &= ~(1ul << (i % BitSize));
    }

    public bool IsSet(int i)
    {
        if (i < 0 || i >= _bits.Length * BitSize) return false;

        return ((_bits[i / BitSize] >> (i % BitSize)) & 1ul) > 0;
    }

    public void Clear()
    {
        _bits = new ulong[1];
        Count = 0;
    }

    public void ShiftLeft(int index)
    {
        if (index < 0 || index >= _bits.Length * BitSize) return;

        var n = index / BitSize;
        var buffer = 0ul;
        for (int j = 0; j < _bits.Length; j++)
        {
            if (j < n) continue;
            
            var bufferBuffer = (_bits[j] >> (BitSize - 1)) & 1ul;
            if (j == n)
            {
                var x = _bits[j];
                var y = x;
                var l = index % BitSize;
                var mask = ~(ulong.MaxValue << l);

                x &= mask;
                y = (y & ~mask) << 1;
                _bits[j] = x | y;
            }
            else
            {
                _bits[j] = (_bits[j] << 1) | buffer;
            }

            buffer = bufferBuffer;
        }

        if (buffer != 1) return;
        
        var array = new ulong[_bits.Length + 1];
        Array.Copy(_bits, 0, array, 0, _bits.Length);
        array[^1] = 1;
        _bits = array;
    }

    public void ShiftRight(int index)
    {
        if (index < 0 || index >= _bits.Length * BitSize) return;

        var n = index / BitSize;
        for (int j = 0; j < _bits.Length; j++)
        {
            if (j < n) continue;
            
            if (j == n)
            {
                var x = _bits[j];
                var y = x;
                var l = index % BitSize;
                var mask = ~(ulong.MaxValue << l);

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

    public void Or(InfiniteBitSet bitSet)
    {
        GrowIfNecessary(bitSet._bits.Length);

        int count = 0;
        for (int i = 0; i < _bits.Length; i++)
        {
            _bits[i] |= bitSet._bits[i];
            count += System.Numerics.BitOperations.PopCount(_bits[i]);
        }

        Count = count;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var l in _bits)
        {
            for (int i = 63; i >= 0; i--)
            {
                builder.Append(((l >> i) & 1) > 0 ? '1' : '0');
            }

            builder.Append('\n');
        }
        
        return builder.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < _bits.Length; i++)
        {
            if (_bits[i] == 0) continue;

            for (int j = 0; j < BitSize; j++)
            {
                if (((_bits[i] >> j) & 1) > 0) yield return i * BitSize + j;
            }
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not InfiniteBitSet bitSet || bitSet._bits.Length != _bits.Length) return false;

        for (int i = 0; i < _bits.Length; i++)
        {
            if (_bits[i] != bitSet._bits[i]) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var b in _bits)
        {
            hash ^= (int)(b & 0xFFFFFFFF) ^ (int)((b >> 32) & 0xFFFFFFFF);
        }

        return hash;
    }

    private void GrowIfNecessary(int arrayLength)
    {
        if (arrayLength <= _bits.Length) return;
        
        var buffer = new ulong[arrayLength];
        Array.Copy(_bits, 0, buffer, 0, _bits.Length);
        _bits = buffer;
    }
}