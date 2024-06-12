using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

namespace Model.Sudokus.Solver.Utility.Graphs;

public class SudokuConstructRuleBank : IConstructRuleBank<ISudokuStrategyUser, ISudokuElement>
{
    public const int UnitStrongLink = 0,
        CellStrongLink = 1,
        UnitWeakLink = 2,
        CellWeakLink = 3,
        PointingPossibilities = 4,
        AlmostNakedPossibilities = 5,
        XYChainSpecific = 6,
        JuniorExocet = 7;
    
    private readonly IConstructRule<ISudokuStrategyUser, ISudokuElement>[] _rules =
    {
        new UnitStrongLinkConstructRule(),
        new CellStrongLinkConstructRule(),
        new UnitWeakLinkConstructRule(),
        new CellWeakLinkConstructRule(),
        new PointingPossibilitiesConstructRule(),
        new AlmostNakedSetConstructRule(),
        new XYChainSpecificConstructRule(),
        new JuniorExocetConstructRule()
    };

    public IConstructRule<ISudokuStrategyUser, ISudokuElement> this[int index] => _rules[index];
}