namespace Model.Core.Graphs;

public class ConstructedGraph<TDataSource, TGraph> where TGraph : IClearable
{
    public TGraph Graph { get; }
    private ulong _rulesApplied;
    private readonly TDataSource _dataSource;

    public ConstructedGraph(TGraph graph, TDataSource dataSource)
    {
        Graph = graph;
        _dataSource = dataSource;
    }

    public void Construct(params IConstructionRule<TDataSource, TGraph>[] rules)
    {
        if(IsOverConstructed(rules)) Clear();
        foreach (var rule in rules)
        {
            DoConstruct(rule);
        }
    }

    public void Clear()
    {
        Graph.Clear();
        _rulesApplied = 0;
    }
    
    private void DoConstruct(IConstructionRule<TDataSource, TGraph> rule)
    {
        if(((_rulesApplied >> rule.ID) & 1) > 0) return;

        rule.Apply(Graph, _dataSource);
        _rulesApplied |= 1UL << rule.ID;
    }
    
    private bool IsOverConstructed(params IConstructionRule<TDataSource, TGraph>[] rules)
    {
        var buffer = 0UL;
        foreach (var rule in rules)
        {
            buffer |= 1UL << rule.ID;
        }

        return (buffer | _rulesApplied) != buffer;
    }
}