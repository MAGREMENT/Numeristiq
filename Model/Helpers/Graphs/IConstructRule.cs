using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Helpers.Graphs;

public interface IConstructRule<in TDataSource, TComplexType> where TComplexType : notnull
{
    void Apply(ILinkGraph<TComplexType> linkGraph, TDataSource strategyUser);
    
    void Apply(ILinkGraph<CellPossibility> linkGraph, TDataSource strategyUser);
}