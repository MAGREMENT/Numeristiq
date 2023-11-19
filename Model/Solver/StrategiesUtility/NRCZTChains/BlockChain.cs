using System;
using System.Collections.Generic;

namespace Model.Solver.StrategiesUtility.NRCZTChains;

public class BlockChain : List<Block>
{
    public void RemoveLast()
    {
        if (Count == 0) return;

        RemoveAt(Count - 1);
    }

    public Block Last()
    {
        if (Count == 0) throw new Exception();

        return this[^1];
    }

    public HashSet<CellPossibility> AllCellPossibilities()
    {
        var result = new HashSet<CellPossibility>(Count);

        for (int i = 0; i < Count; i++)
        {
            result.Add(this[i].Start);
            result.Add(this[i].End);
        }

        return result;
    }

    public BlockChain Copy()
    {
        var copy = new BlockChain();
        copy.AddRange(this);
        return copy;
    }
}