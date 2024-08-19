namespace Model.Core.Graphs;

public interface IConstructionRule<in TDataSource, in TGraph> where TGraph : notnull
{ 
    int ID { get; }
    
    void Apply(TGraph linkGraph, TDataSource data);
}

public static class UniqueConstructionRuleID
{
    private static int _current;

    public static int Next() => _current++;
}