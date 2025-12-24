using System.Collections.Generic;

namespace Model.Core.Syntax;

public interface ISyntaxInterpreter<TResult, TError> where TError : ISyntaxError
{
    public TError? TryInterpret(IReadOnlyList<ParsedLine> lines, out TResult result);
}