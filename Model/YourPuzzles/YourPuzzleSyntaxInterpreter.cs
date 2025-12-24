

using System.Collections.Generic;
using Model.Core.Syntax;

namespace Model.YourPuzzles;

public class YourPuzzleSyntaxInterpreter : ISyntaxInterpreter<INumericPuzzleRule, ISyntaxError>
{
    public ISyntaxError? TryInterpret(IReadOnlyList<ParsedLine> lines, out INumericPuzzleRule result)
    {
        throw new System.NotImplementedException();
    }
}