using System;
using Model.StrategiesUtil.LinkGraph.ConstructRules;

namespace Model.Solver.StrategiesUtil.LinkGraph;

public class LinkGraphManager //TODO : Refactor chaining strategies, cache the linkgraph
{
    public LinkGraph<ILinkGraphElement> LinkGraph { get; } = new();

    private readonly IStrategyManager _solver; //TODO make into interface

    private long _rulesApplied = 0;
    private readonly IConstructRule[] _rules =
    {
        new UnitStrongLinkConstructRule(),
        new CellStrongLinkConstructRule(),
        new UnitWeakLinkConstructRule(),
        new CellWeakLinkConstructRule(),
        new PointingPossibilitiesConstructRule(),
        new AlmostNakedPossibilitiesConstructRule()
    };

    public LinkGraphManager(IStrategyManager solver)
    {
        _solver = solver;
    }

    public void Construct(ConstructRule rule)
    {
        int asInt = (int)rule;
        if(((_rulesApplied >> asInt) & 1) > 0) return;

        _rules[asInt].Apply(LinkGraph, _solver);
        _rulesApplied |= 1L << asInt;
    }

    public void Construct(params ConstructRule[] rules)
    {
        foreach (var rule in rules)
        {
            Construct(rule);
        }
    }

    public void Construct()
    {
        foreach (var rule in Enum.GetValues<ConstructRule>())
        {
            Construct(rule);
        }
    }

    public void Clear()
    {
        LinkGraph.Clear();
        _rulesApplied = 0;
    }
}

public enum ConstructRule
{
    UnitStrongLink = 0, CellStrongLink = 1, UnitWeakLink = 2, CellWeakLink = 3, PointingPossibilities = 4, AlmostNakedPossibilities = 5
}

public interface IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager);
}