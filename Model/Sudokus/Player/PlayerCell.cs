using System.Collections.Generic;
using Model.Utility.BitSets;

namespace Model.Sudokus.Player;

public struct PlayerCell //TODO make readonly
{
    /*
     * 0-8 bottom possibilities
     * 9-17 possibilities
     * 18-26 top possibilities
     * 27-31 number
     */
    private int _bits;
    public bool Editable { get; set; }

    public PlayerCell(bool editable)
    {
        Editable = editable;
        _bits = 0;
    }

    public bool IsNumber()
    {
        return ((_bits >> 27) & 0xF) > 0;
    }

    public int Number()
    {
        return (_bits >> 27) & 0xF;
    }

    public void SetNumber(int n)
    {
        if (!Editable) return;
        
        _bits = n << 27;
    }

    public void RemoveNumber(int n)
    {
        if (!Editable) return;

        if (Number() == n) _bits = 0;
    }

    public void RemoveNumber()
    {
        if (!Editable) return;

        _bits = 0;
    }

    public IEnumerable<int> Possibilities(PossibilitiesLocation location)
    {
        var buffer = (_bits >> (int)location) & 0x1FF;
        for (int i = 0; i < 9; i++)
        {
            if (Peek(buffer, i)) yield return i + 1;
        }
    }

    public ReadOnlyBitSet16 PossibilitiesAsBitSet(PossibilitiesLocation location)
    {
        var buffer = (_bits >> (int)location) & 0x1FF;
        return ReadOnlyBitSet16.FromBits((ushort) (buffer << 1));
    }

    public void AddPossibility(int possibility, PossibilitiesLocation location)
    {
        if (!Editable) return;

        var delta = possibility - 1 + (int)location;
        Set(delta);
    }
    
    public void RemovePossibility(int possibility, PossibilitiesLocation location)
    {
        if (!Editable) return;

        var delta = possibility - 1 + (int)location;
        UnSet(delta);
    }

    public bool PeekPossibility(int possibility, PossibilitiesLocation location)
    {
        var delta = possibility - 1 + (int)location;
        return Peek(_bits, delta);
    }

    public int PossibilitiesCount(PossibilitiesLocation location)
    {
        var buffer = (_bits >> (int)location) & 0x1FF;
        return System.Numerics.BitOperations.PopCount((uint)buffer);
    }

    public void RemovePossibility(PossibilitiesLocation location)
    {
        if (!Editable) return;

        for (int i = 8; i >= 0; i--)
        {
            if (!Peek(_bits, (int)location + i)) continue;
            
            UnSet((int)location + i);
            break;
        }
    }

    public void SetPossibilities(ReadOnlyBitSet16 bitSet, PossibilitiesLocation location)
    {
        if (!Editable) return;

        var buffer = (bitSet.Bits >> 1) << (int)location;
        _bits &= 0x1FF << (int)location;
        _bits |= buffer;
    }

    public void FillPossibilities(PossibilitiesLocation location)
    {
        if (!Editable) return;

        _bits |= 0x1FF << (int)location;
    }

    public bool IsEmpty()
    {
        return _bits == 0;
    }

    public void Empty()
    {
        if (!Editable) return;
        
        _bits = 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is PlayerCell pc && pc == this;
    }

    public override int GetHashCode()
    {
        return _bits;
    }

    public static bool operator ==(PlayerCell left, PlayerCell right)
    {
        return left._bits == right._bits;
    }

    public static bool operator !=(PlayerCell left, PlayerCell right)
    {
        return !(left == right);
    }

    //Private-----------------------------------------------------------------------------------------------------------
    
    private static bool Peek(int bits, int n)
    {
        return ((bits >> n) & 1) > 0;
    }

    private void Set(int n)
    {
        _bits |= 1 << n;
    }

    private void UnSet(int n)
    {
        _bits &= ~(1 << n);
    }
}

public enum PossibilitiesLocation
{
    Bottom = 0, Middle = 9, Top = 18
}