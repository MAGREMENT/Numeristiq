namespace Model.Core.Graphs;

public class ManagedLinkGraph<TDataSource, TType> where TType : notnull
{
    public ILinkGraph<TType> Graph { get; }
    private ulong _rulesApplied;
    private readonly TDataSource _dataSource;

    public ManagedLinkGraph(ILinkGraph<TType> graph, TDataSource dataSource)
    {
        Graph = graph;
        _dataSource = dataSource;
    }

    public void Construct(params IConstructRule<TDataSource, TType>[] rules)
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
    
    private void DoConstruct(IConstructRule<TDataSource, TType> rule)
    {
        if(((_rulesApplied >> rule.ID) & 1) > 0) return;

        rule.Apply(Graph, _dataSource);
        _rulesApplied |= 1UL << rule.ID;
    }
    
    private bool IsOverConstructed(params IConstructRule<TDataSource, TType>[] rules)
    {
        var buffer = 0UL;
        foreach (var rule in rules)
        {
            buffer |= 1UL << rule.ID;
        }

        return (buffer | _rulesApplied) != buffer;
    }
}