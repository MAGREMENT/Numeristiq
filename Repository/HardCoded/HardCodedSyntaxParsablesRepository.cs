using Model.Repositories;
using Model.YourPuzzles.Syntax;
using Model.YourPuzzles.Syntax.Operators;
using Model.YourPuzzles.Syntax.Parsables;
using Model.YourPuzzles.Syntax.Values;

namespace Repository.HardCoded;

public class HardCodedSyntaxParsablesRepository : ISyntaxParsablesRepository
{
    private static readonly ISyntaxParsable[] _arr =
    {
        new CellParsable(),
        new IntParsable(),
        
        new SimpleStringParsable<EqualsOperator>("="),
        new SimpleStringParsable<GreaterThanOperator>(">"),
        new SimpleStringParsable<AddOperator>("+"),
        new SimpleStringParsable<MultiplyOperator>("*")
    };

    public IEnumerable<ISyntaxParsable> GetParsables()
    {
        return _arr;
    }
}