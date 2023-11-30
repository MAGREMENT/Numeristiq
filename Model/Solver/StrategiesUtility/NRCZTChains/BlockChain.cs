using System;
using System.Collections.Generic;
using System.Text;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.NRCZTChains;

public class BlockChain : List<Block>
{
    public HashSet<CellPossibility> PossibleTargets { get; } = new();

    private readonly Dictionary<Block, List<CellPossibility>> _removed = new();
    
    public BlockChain(Block first, LinkGraph<CellPossibility> graph)
    {
        Add(first);
        foreach (var friend in graph.GetLinks(first.Start))
        {
            if (friend != first.End) PossibleTargets.Add(friend);
        }
    }
    
    private BlockChain(){}

    public new void Add(Block b)
    {
        base.Add(b);
        List<CellPossibility> removed = new();
        if(PossibleTargets.Remove(b.Start)) removed.Add(b.Start);
        if(PossibleTargets.Remove(b.End)) removed.Add(b.End);
        _removed[b] = removed;
    }
    
    public void RemoveLast(LinkGraph<CellPossibility> graph)
    {
        if (Count == 0) return;

        var b = this[Count - 1];
        foreach (var removed in _removed[b])
        {
            PossibleTargets.Add(removed);
        }
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

    public bool IsWeaklyLinkedToAtLeastOneEnd(CellPossibility cp)
    {
        foreach (var block in this)
        {
            if (cp.Possibility == block.End.Possibility)
            {
                if(Cells.ShareAUnit(block.End.ToCell(), cp.ToCell())) return true;
            }
            else
            {
                if (cp.Row == block.End.Row && cp.Column == block.End.Column) return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        if (Count == 0) return "";
        
        var builder = new StringBuilder();
        for (int i = 0; i < Count - 1; i++)
        {
            builder.Append(this[i] + " - ");
        }

        builder.Append(Last().ToString());

        return builder.ToString();
    }
}