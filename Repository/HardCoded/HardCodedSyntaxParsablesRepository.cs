using Model.Core.Syntax;
using Model.Core.Syntax.Operators;
using Model.Core.Syntax.Parsables;
using Model.Core.Syntax.Values;
using Model.Repositories;

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
        new SimpleStringParsable<MultiplyOperator>("*"),
        new SimpleStringParsable<PowerOperator>("^")
    };

    public IEnumerable<ISyntaxParsable> GetParsables()
    {
        return _arr;
    }
}