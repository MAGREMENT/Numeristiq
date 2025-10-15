using System.Collections.Generic;
using Model.Core.Syntax;

namespace Model.Repositories;

public interface ISyntaxParsablesRepository<T> where T : ISyntaxElement
{
    IEnumerable<ISyntaxParsable<T>> GetParsables();
}