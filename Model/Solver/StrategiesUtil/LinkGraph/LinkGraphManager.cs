using System;
using Model.Solver.StrategiesUtil.LinkGraph.ConstructRules;

namespace Model.Solver.StrategiesUtil.LinkGraph;

public class LinkGraphManager //TODO : Refactor chaining strategies
{
    public LinkGraph<ILinkGraphElement> LinkGraph { get; } = new();

    private readonly IStrategyManager _solver;

    private long _rulesApplied;
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
        IsOverConstructed(rule);
        DoConstruct(rule);
    }

    public void Construct(params ConstructRule[] rules)
    {
        IsOverConstructed(rules);
        foreach (var rule in rules)
        {
            DoConstruct(rule);
        }
    }

    public void Construct()
    {
        foreach (var rule in Enum.GetValues<ConstructRule>())
        {
            DoConstruct(rule);
        }
    }

    public void Clear()
    {
        LinkGraph.Clear();
        _rulesApplied = 0;
    }
    
    private void DoConstruct(ConstructRule rule)
    {
        int asInt = (int)rule;
        if(((_rulesApplied >> asInt) & 1) > 0) return;

        _rules[asInt].Apply(LinkGraph, _solver);
        _rulesApplied |= 1L << asInt;
    }

    private void IsOverConstructed(params ConstructRule[] rules)
    {
        var buffer = 0L;
        foreach (var rule in rules)
        {
            buffer |= 1L << (int)rule;
        }

        for (int i = 0; i < _rules.Length; i++)
        {
            if (((_rulesApplied >> i) & 1) > 0 && ((buffer >> i) & 1) == 0)
            {
                Clear();
                return;
            }
        }
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