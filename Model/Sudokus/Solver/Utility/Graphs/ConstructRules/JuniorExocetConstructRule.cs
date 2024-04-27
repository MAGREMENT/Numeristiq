using Model.Helpers;
using Model.Helpers.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class JuniorExocetConstructRule : IConstructRule<ISudokuStrategyUser, ISudokuElement> //TODO
{
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, ISudokuStrategyUser strategyUser)
    {
        
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ISudokuStrategyUser strategyUser)
    {
        
    }
}