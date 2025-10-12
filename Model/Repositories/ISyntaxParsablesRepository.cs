using System.Collections.Generic;
using Model.YourPuzzles.Syntax;

namespace Model.Repositories;

public interface ISyntaxParsablesRepository
{
    IEnumerable<ISyntaxParsable> GetParsables();
}