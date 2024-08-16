namespace Model.Core.Graphs;

public interface IConstructRule<in TDataSource, TType> where TType : notnull
{ 
    int ID { get; }
    
    void Apply(IGraph<TType, LinkStrength> linkGraph, TDataSource data);
}

public class UniqueConstructRuleID
{
    private static int _current;

    public static int Next() => _current++;
}