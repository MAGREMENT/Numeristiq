using System;
using Model.Solver.Possibilities;

namespace Model.Solver.StrategiesUtil.LinkGraph.ConstructRules;

public class JuniorExocetConstructRule : IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager)
    {
        foreach (var je in strategyManager.PreComputer.JuniorExocet())
        {
            var bc = new[] { je.Base1, je.Base2 };
            foreach (var possibility in je.BaseCandidates)
            {
                if (!strategyManager.PossibilitiesAt(je.Target1).Peek(possibility)
                    || !strategyManager.PossibilitiesAt(je.Target2).Peek(possibility)) continue;

                linkGraph.AddLink(new JuniorExocetTargetPossibility(possibility, je.Target1, bc),
                    new JuniorExocetTargetPossibility(possibility, je.Target2, bc), LinkStrength.Weak);
            }
        }
    }

    public void Apply(LinkGraph<CellPossibility> linkGraph, IStrategyManager strategyManager)
    {
        
    }
}

public class JuniorExocetTargetPossibility : ILinkGraphElement
{
    public int Possibility { get; }
    public int Row { get; }
    public int Col { get; }
    public Cell[] BaseCells { get; }
    
    public JuniorExocetTargetPossibility(int possibility, Cell cell, Cell[] baseCells)
    {
        Possibility = possibility;
        Row = cell.Row;
        Col = cell.Col;
        BaseCells = baseCells;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, Row, Col);
    }

    public override bool Equals(object? obj)
    {
        return (obj is CellPossibility cp && cp.Possibility == Possibility && cp.Row == Row && cp.Col == Col) ||
               (obj is ICellPossibility icp && icp.Possibility == Possibility && icp.Row == Row && icp.Col == Col);
    }

    public override string ToString()
    {
        return $"JE : {Possibility}[{Row + 1}, {Col + 1}]";
    }

    public CellPossibilities[] EveryCellPossibilities()
    {
        return new[] { new CellPossibilities(new Cell(Row, Col), Possibility) };
    }

    public Cell[] EveryCell()
    {
        return new Cell[] { new(Row, Col) };
    }

    public IPossibilities EveryPossibilities()
    {
        var result = IPossibilities.NewEmpty();
        result.Add(Possibility);
        return result;
    }
}