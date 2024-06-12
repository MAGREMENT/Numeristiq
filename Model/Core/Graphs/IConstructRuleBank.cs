namespace Model.Core.Graphs;

public interface IConstructRuleBank<in TDataSource, TComplexType> where TComplexType : notnull
{
    public IConstructRule<TDataSource, TComplexType> this[int index] { get; }
}