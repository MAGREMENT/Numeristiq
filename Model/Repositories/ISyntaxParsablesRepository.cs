using System.Collections.Generic;
using Model.Core.Syntax;

namespace Model.Repositories;

public interface ISyntaxParsablesRepository
{
    IEnumerable<ISyntaxParsable> GetParsables();
}