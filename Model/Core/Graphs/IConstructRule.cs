namespace Model.Core.Graphs;

public interface IConstructRule<in TDataSource, TType> where TType : notnull
{
    int ID { get; }
    
    void Apply(ILinkGraph<TType> linkGraph, TDataSource strategyUser);
}