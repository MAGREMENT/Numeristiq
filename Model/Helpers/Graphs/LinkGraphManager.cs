using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Helpers.Graphs;

public class LinkGraphManager<TDataSource, TComplexType> where TComplexType : notnull
{
    public ILinkGraph<TComplexType> ComplexLinkGraph { get; } = LinkGraphInstantiator.For<TComplexType>();
    private long _rulesAppliedOnComplex;

    public ILinkGraph<CellPossibility> SimpleLinkGraph { get; } = LinkGraphInstantiator.For<CellPossibility>();
    private long _rulesAppliedOnSimple;

    private readonly TDataSource _solver;
    private readonly IConstructRuleBank<TDataSource, TComplexType> _bank;

    public LinkGraphManager(TDataSource solver, IConstructRuleBank<TDataSource, TComplexType> bank)
    {
        _solver = solver;
        _bank = bank;
    }

    public void ConstructComplex(params int[] rules)
    {
        if(IsOverConstructed(_rulesAppliedOnComplex, rules)) ClearComplex();
        foreach (var rule in rules)
        {
            DoConstructComplex(rule);
        }
    }

    public void ConstructSimple(params int[] rules)
    {
        if(IsOverConstructed(_rulesAppliedOnSimple, rules)) ClearSimple();
        foreach (var rule in rules)
        {
            DoConstructSimple(rule);
        }
    }

    public void Clear()
    {
        ClearComplex();
        ClearSimple();
    }
    
    private void DoConstructComplex(int rule)
    {
        if(((_rulesAppliedOnComplex >> rule) & 1) > 0) return;

        _bank[rule].Apply(ComplexLinkGraph, _solver);
        _rulesAppliedOnComplex |= 1L << rule;
    }

    private void DoConstructSimple(int rule)
    {
        if(((_rulesAppliedOnSimple >> rule) & 1) > 0) return;

        _bank[rule].Apply(SimpleLinkGraph, _solver);
        _rulesAppliedOnSimple |= 1L << rule;
    }

    private void ClearComplex()
    {
        ComplexLinkGraph.Clear();
        _rulesAppliedOnComplex = 0;
    }

    private void ClearSimple()
    {
        SimpleLinkGraph.Clear();
        _rulesAppliedOnSimple = 0;
    }

    private bool IsOverConstructed(long rulesApplied, params int[] rules)
    {
        var buffer = 0L;
        foreach (var rule in rules)
        {
            buffer |= 1L << rule;
        }

        return (buffer | rulesApplied) != buffer;
    }
}

