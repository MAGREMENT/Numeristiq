using System;
using System.Collections.Generic;
using System.Text;
using Model.Solver.StrategiesUtility.LinkGraph;

namespace Model.Solver.StrategiesUtility.NRCZTChains;

public class BlockChain : List<Block>
{
    public HashSet<CellPossibility> PossibleTargets { get; } = new();
    
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
        PossibleTargets.Remove(b.Start);
        PossibleTargets.Remove(b.End);
    }
    
    public void RemoveLast(LinkGraph<CellPossibility> graph)
    {
        if (Count == 0) return;

        var b = this[Count - 1];
        if (graph.HasLinkTo(b.Start, this[0].Start)) PossibleTargets.Add(b.Start);
        if (graph.HasLinkTo(b.End, this[0].Start)) PossibleTargets.Add(b.End);
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
                if (cp.Row == block.End.Row && cp.Col == block.End.Col) return true;
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